using System; 
using Cysharp.Threading.Tasks;
using UnityEngine;
using Oculus.Platform;
public class GamePurchasedValidator : MonoBehaviour
{
   private bool _initialized;
   private bool _failedInitialize;

   private const string HEADER = "Failed to validate purchase";

   private const string MESSAGE =
      "It looks like you may not have purchased this game. If you enjoy this game I would really appreciate your support by buying it when you are able.";
   private const string CONFIRM = "Continue";
      
   private void Start()
   {
      Validate();
   }

#if UNITY_ANDROID //Oculus specific
   private void Validate()
   {
      try
      {
         var request = Core.AsyncInitialize(PlatformSettings.MobileAppID);
         request.OnComplete((initializer) =>
         {
            if (initializer.IsError)
            {
               Debug.LogError(initializer.Data.Result);
               _failedInitialize = true;
               return;
            }

            Entitlements.IsUserEntitledToApplication().OnComplete((entitlementCheck) =>
            {
               if (entitlementCheck.IsError)
               {
                  RequestNotification();
               }
               else
               {
                  Debug.Log("User entitlement succeeded");
               }
            });
         });
      }
      catch (Exception e)
      {
         Debug.LogError(e);
         _initialized = false;
         _failedInitialize = true;
         RequestNotification();
      }
   }
   #else
    private void Validate()
   {
      
   }
   #endif

   private void RequestNotification()
   {
      var visuals = new Notification.NotificationVisuals(MESSAGE, HEADER, CONFIRM, disableUI:true);
      NotificationManager.RequestNotification(visuals);
   }
}
