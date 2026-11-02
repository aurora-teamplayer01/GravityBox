import android.os.SystemClock;
import android.view.InputDevice;
import android.view.InputEvent;
import android.view.MotionEvent;
import java.lang.reflect.Method;

/** Emulator-only smoke helper: injects ordinary two-finger Android touch events. */
public final class AlienPetPinch {
    private static Object manager;
    private static Method inject;
    private static long down;
    private static void event(int action, int count, float x0, float x1, float y) throws Exception {
        MotionEvent.PointerProperties[] props = new MotionEvent.PointerProperties[count];
        MotionEvent.PointerCoords[] coords = new MotionEvent.PointerCoords[count];
        for (int i = 0; i < count; i++) {
            props[i] = new MotionEvent.PointerProperties(); props[i].id = i;
            props[i].toolType = MotionEvent.TOOL_TYPE_FINGER;
            coords[i] = new MotionEvent.PointerCoords(); coords[i].x = i == 0 ? x0 : x1;
            coords[i].y = y; coords[i].pressure = 1; coords[i].size = 0.1f;
        }
        MotionEvent e = MotionEvent.obtain(down, SystemClock.uptimeMillis(), action, count,
            props, coords, 0, 0, 1, 1, 0, 0, InputDevice.SOURCE_TOUCHSCREEN, 0);
        if (!(Boolean) inject.invoke(manager, e, 2)) throw new IllegalStateException("Touch injection refused");
        e.recycle();
    }
    public static void main(String[] args) throws Exception {
        Class<?> type = Class.forName("android.hardware.input.InputManagerGlobal");
        manager = type.getMethod("getInstance").invoke(null);
        inject = type.getMethod("injectInputEvent", InputEvent.class, int.class);
        float center = Float.parseFloat(args[0]), y = Float.parseFloat(args[1]);
        float start = Float.parseFloat(args[2]), end = Float.parseFloat(args[3]);
        down = SystemClock.uptimeMillis();
        event(MotionEvent.ACTION_DOWN, 1, center-start, center+start, y);
        event(MotionEvent.ACTION_POINTER_DOWN | (1 << MotionEvent.ACTION_POINTER_INDEX_SHIFT), 2, center-start, center+start, y);
        for (int i = 1; i <= 24; i++) {
            float spread = start + (end-start)*i/24f;
            event(MotionEvent.ACTION_MOVE, 2, center-spread, center+spread, y); SystemClock.sleep(25);
        }
        event(MotionEvent.ACTION_POINTER_UP | (1 << MotionEvent.ACTION_POINTER_INDEX_SHIFT), 2, center-end, center+end, y);
        event(MotionEvent.ACTION_UP, 1, center-end, center+end, y);
        System.out.println("Two-finger pinch injection completed");
    }
}
