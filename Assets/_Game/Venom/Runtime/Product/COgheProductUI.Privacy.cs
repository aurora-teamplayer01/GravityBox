using System;
using UnityEngine;
using UnityEngine.UI;

namespace GravityBox.Venom
{
    /// <summary>Neutral, local age choice before the first intro or returning-player menu.
    /// The privacy service owns SDK eligibility; these controls never grant rewards or change puzzle progress.</summary>
    public sealed partial class COgheProductUI
    {
        public const string PrivacyPolicyUrl = "https://cj.suri.house/alien-pet-privacy";
        // Existing isolated gameplay tests disable persistence; dedicated privacy tests explicitly enable this gate.
        public static bool? PrivacyGateForTests;
        public static Action<string> PrivacyOpenUrlForTests;
        public bool PrivacySetupRequired => (PrivacyGateForTests ?? VenomCampaignSave.PersistenceEnabled) && !COghePrivacy.SetupComplete;
        private bool HideAdOffers => (PrivacyGateForTests ?? VenomCampaignSave.PersistenceEnabled) && !COghePrivacy.AdsAgeEligible;
        private COghePrivacy.AgeBand pendingAge;

        private void BeginPrivacySetup()
        {
            pendingAge=COghePrivacy.AgeBand.Unknown;
            Page=COgheProductPage.PrivacySetup;Popup=COgheProductPopup.None;
            SetPaused(true);COgheAds.Banner(false);Rebuild();
        }

        private void FinishPrivacySetup()
        {
            if(!COghePrivacy.SetupComplete)return;
            Page=COgheProductPage.Game;SetPaused(false);StartPlayerSession();
        }

        private void ConfirmPrivacyAge()
        {
            if(pendingAge==COghePrivacy.AgeBand.Unknown || COghePrivacy.AgeKnown)return;
            COghePrivacy.SetAgeBand(pendingAge);
            if(COghePrivacy.SetupComplete)FinishPrivacySetup();else Rebuild();
        }

        private void ChooseAnalytics(bool allowed)
        {
            COghePrivacy.SetAnalyticsChoice(allowed);
            if(Page==COgheProductPage.PrivacySetup)FinishPrivacySetup();else Rebuild();
        }

        private static string AgeLabel(COghePrivacy.AgeBand age) => age switch
        {
            COghePrivacy.AgeBand.Under13 => "Under 13",
            COghePrivacy.AgeBand.Age13To15 => "13–15",
            COghePrivacy.AgeBand.Age16To17 => "16–17",
            COghePrivacy.AgeBand.Adult => "18 or older",
            _ => "Not selected"
        };

        private void OpenPrivacyPolicy()
        {
            if(PrivacyOpenUrlForTests!=null)PrivacyOpenUrlForTests(PrivacyPolicyUrl);
            else Application.OpenURL(PrivacyPolicyUrl);
        }

        private Button PrivacyTextButton(Transform parent,string name,Rect rect,string label,Action action,bool selected=false)
        {
            var image=art.Box(parent,name,rect,selected?COgheUIArt.Mint:new Color(.995f,.995f,.972f),true);
            image.pixelsPerUnitMultiplier=2;
            var button=image.gameObject.AddComponent<Button>();button.targetGraphic=image;
            button.onClick.AddListener(()=>{COgheAudio.UiTap();action();});
            art.Label(image.transform,"Label",label,new Rect(12,0,rect.width-24,rect.height),16);
            if(selected)art.Icon(image.transform,COgheIcon.Check,new Rect(rect.width-35,(rect.height-22)*.5f,22,22),COgheUIArt.Teal);
            return button;
        }

        private void PrivacySetupView()
        {
            // Full opaque paper keeps the first choice calm, without gameplay, hints or monster imagery behind it.
            art.Box(pageRoot,"Privacy backdrop",new Rect(-width,-height,width*3,height*3),COgheUIArt.Paper,true).sprite=null;
            bool ageStep=!COghePrivacy.AgeKnown;
            float w=Mathf.Min(328,width-32),h=ageStep?532:392;
            var panel=art.Rect(pageRoot,"Privacy setup",new Rect((width-w)*.5f,(height-h)*.5f,w,h));
            art.Label(panel,"Brand",AlienPetBrand.Name,new Rect(16,0,w-32,38),27,COgheUIArt.Teal);
            art.Label(panel,"Title",ageStep?"How old is the player?":"Help improve Alien Pet?",new Rect(12,54,w-24,38),21);
            art.Label(panel,"Explanation",ageStep?"We use this to choose\nage-appropriate settings.":"Optional game analytics helps us understand\nwhich puzzles need improving. You can play\nwithout it and change this in Privacy.",new Rect(12,100,w-24,ageStep?48:80),14,COgheUIArt.Muted);
            if(ageStep)
            {
                var ages=new[]{COghePrivacy.AgeBand.Under13,COghePrivacy.AgeBand.Age13To15,COghePrivacy.AgeBand.Age16To17,COghePrivacy.AgeBand.Adult};
                for(int i=0;i<ages.Length;i++)
                {
                    var choice=ages[i];
                    PrivacyTextButton(panel,"Age "+choice,new Rect(20,164+i*62,w-40,52),AgeLabel(choice),()=>{pendingAge=choice;Rebuild();},pendingAge==choice);
                }
                PrivacyTextButton(panel,"Confirm age",new Rect(20,424,w-40,52),"Continue",ConfirmPrivacyAge).interactable=pendingAge!=COghePrivacy.AgeBand.Unknown;
            }
            else
            {
                // Equal size and emphasis; no preselected agreement, incentives or mandatory consent.
                PrivacyTextButton(panel,"Analytics no",new Rect(20,202,w-40,52),"Not now",()=>ChooseAnalytics(false));
                PrivacyTextButton(panel,"Analytics yes",new Rect(20,266,w-40,52),"Allow analytics",()=>ChooseAnalytics(true));
            }
            PrivacyTextButton(panel,"Privacy policy",new Rect(20,h-48,w-40,48),"Privacy policy",OpenPrivacyPolicy);
        }

        private void PrivacySettingsView()
        {
            bool analytics=COghePrivacy.CanChooseAnalytics;
            bool adChoices=COgheAds.PrivacyRequired?.Invoke()==true;
            float w=Mathf.Min(328,width-32),h=354+(analytics?78:0)+(adChoices?58:0);
            var panel=art.Box(popupRoot,"Privacy panel",new Rect((width-w)*.5f,(height-h)*.5f,w,h),COgheUIArt.Paper,true).rectTransform;
            PopupTitle(panel,w,"Privacy");
            art.Label(panel,"Age group","Player age: "+AgeLabel(COghePrivacy.Age),new Rect(20,81,w-40,26),14);
            art.Label(panel,"Age correction","For help with this choice, a parent or\nguardian can contact support@suri.house.",new Rect(20,116,w-40,55),12,COgheUIArt.Muted);
            PrivacyTextButton(panel,"Privacy policy",new Rect(24,188,w-48,48),"Privacy policy",OpenPrivacyPolicy);
            float y=248;
            if(analytics)
            {
                string label=COghePrivacy.AnalyticsAllowed?"Analytics: On · turn off":"Analytics: Off · turn on";
                PrivacyTextButton(panel,"Analytics choice",new Rect(24,y,w-48,52),label,()=>ChooseAnalytics(!COghePrivacy.AnalyticsAllowed));
                y+=78;
            }
            if(adChoices)
            {
                PrivacyTextButton(panel,"Ad privacy choices",new Rect(24,y,w-48,48),"Ad privacy choices",()=>COgheAds.OpenPrivacy?.Invoke());
            }
            PrivacyTextButton(panel,"Privacy done",new Rect(24,h-72,w-48,48),"Done",()=>{if(Page==COgheProductPage.MainMenu)Resume();else ShowPopup(COgheProductPopup.Pause);});
        }
    }
}
