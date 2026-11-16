// Alien Pet: StoreKit 2 bridge for the two one-time purchases (COghe Plus, No Ads).
// C# side: Assets/_Game/Venom/Runtime/Shop/COgheAppleStore.cs. Every event goes back as (kind, JSON) on the main thread:
//   1 products  {"ids":[...],"prices":[...]}          prices are StoreKit's localized displayPrice
//   2 purchase  {"id":"...","ok":bool,"reason":"..."}  reason: done | cancelled | pending | unverified | unknown | error text
//   3 restore   {"ok":bool}                            ok when at least one entitlement was found
//   4 grant     {"id":"..."}                           a verified, unrevoked entitlement (boot, restore, Ask to Buy, other devices)
//   5 revoke    {"id":"..."}                           refunded or revoked by Apple
import Foundation
import StoreKit

public typealias AlienPetStoreCallback = @convention(c) (Int32, UnsafePointer<CChar>) -> Void

@MainActor private var storeCallback: AlienPetStoreCallback?
@MainActor private var storeProducts: [String: Product] = [:]
@MainActor private var storeUpdates: Task<Void, Never>?

@MainActor private func storeEmit(_ kind: Int32, _ payload: [String: Any]) {
    guard let cb = storeCallback,
          let data = try? JSONSerialization.data(withJSONObject: payload),
          let text = String(data: data, encoding: .utf8) else { return }
    text.withCString { cb(kind, $0) }
}

@MainActor private func storeHandle(_ result: VerificationResult<Transaction>) async {
    guard case .verified(let t) = result else { return }
    if t.revocationDate != nil { storeEmit(5, ["id": t.productID]) }
    else { storeEmit(4, ["id": t.productID]) }
    await t.finish()
}

@MainActor @discardableResult private func storeRefreshEntitlements() async -> Int {
    var found = 0
    for await result in Transaction.currentEntitlements {
        if case .verified(let t) = result, t.revocationDate == nil { found += 1; storeEmit(4, ["id": t.productID]) }
    }
    return found
}

@_cdecl("AlienPetStore_Start")
public func AlienPetStore_Start(_ ids: UnsafePointer<CChar>, _ callback: @escaping AlienPetStoreCallback) {
    let list = String(cString: ids).split(separator: ",").map(String.init)
    Task { @MainActor in
        storeCallback = callback
        if storeUpdates == nil {
            storeUpdates = Task { @MainActor in
                for await update in Transaction.updates { await storeHandle(update) }
            }
        }
        do {
            let products = try await Product.products(for: list)
            var found: [String] = [], prices: [String] = []
            for p in products { storeProducts[p.id] = p; found.append(p.id); prices.append(p.displayPrice) }
            storeEmit(1, ["ids": found, "prices": prices])
        } catch {
            storeEmit(1, ["ids": [String](), "prices": [String](), "error": "\(error)"])
        }
        await storeRefreshEntitlements()
    }
}

@_cdecl("AlienPetStore_Purchase")
public func AlienPetStore_Purchase(_ id: UnsafePointer<CChar>) {
    let pid = String(cString: id)
    Task { @MainActor in
        guard let product = storeProducts[pid] else { storeEmit(2, ["id": pid, "ok": false, "reason": "unknown"]); return }
        do {
            switch try await product.purchase() {
            case .success(.verified(let t)):
                await t.finish()
                storeEmit(2, ["id": pid, "ok": true, "reason": "done"])
            case .success(.unverified(_, _)):
                storeEmit(2, ["id": pid, "ok": false, "reason": "unverified"])
            case .pending:
                storeEmit(2, ["id": pid, "ok": false, "reason": "pending"])
            case .userCancelled:
                storeEmit(2, ["id": pid, "ok": false, "reason": "cancelled"])
            @unknown default:
                storeEmit(2, ["id": pid, "ok": false, "reason": "unknown"])
            }
        } catch {
            storeEmit(2, ["id": pid, "ok": false, "reason": "\(error)"])
        }
    }
}

@_cdecl("AlienPetStore_Restore")
public func AlienPetStore_Restore() {
    Task { @MainActor in
        try? await AppStore.sync()   // asks the player to sign in if needed; cancelling still checks local entitlements
        let found = await storeRefreshEntitlements()
        storeEmit(3, ["ok": found > 0])
    }
}
