using System.Collections;
using System.Collections.Generic;
using System;
using System.Runtime.InteropServices;

using UnityEngine;

public class YUR_SDK : MonoBehaviour
{
    public enum ActivityLevel
    {
        /// Inactive or moving so little as makes no difference
        Inactive = 0,

        /// A light amount of movement activity, enough to represent some effort but not much
        Light = 1,

        /// A fair amount of movement activity, enough to represent what may be considered reasonable exercise
        Moderate = 2,

        /// A significant amount of movement activity, definitely sweating.
        Intense = 3,

        /// A massive amount of effort, going all out
        Vigorous = 4
    };

    /// A player's sex
    public enum Sex
    {
        Female = 0,
        Male = 1,
        Unspecified = 2
    };

    public enum StoreCalcData
    {
        None,
        SaveStart,
        SaveEnd,
        Load,
        RemoveAll
    };
    public enum CalcDataFormat
    {
        Json,
        csv,
        bin,
    };
    public enum SubPlatoform
    {
       Auto = -1, // other than Android platform 
       Android_Mobile,
       Android_Quest,
       Android_Pico,
       Android_Qiyu,
       Android_HTC,
    };


    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public struct Vector3
    {
        Vector3(float x, float y, float z)
        {
            X = x;
            Y = y;
            Z = z;
        }
        /// <summary>
        /// X
        /// </summary>
        [MarshalAs(UnmanagedType.R4)]
        public float X;
        /// <summary>
        /// Y
        /// </summary>
        [MarshalAs(UnmanagedType.R4)]
        public float Y;
        /// <summary>
        /// Z
        /// </summary>
        [MarshalAs(UnmanagedType.R4)]
        public float Z;
    };

    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public struct Quat
    {
        Quat(float x, float y, float z, float w)
        {
            X = x;
            Y = y;
            Z = z;
            W = w;
        }

        /// <summary>
        /// X
        /// </summary>
        [MarshalAs(UnmanagedType.R4)]
        public float X;
        /// <summary>
        /// Y
        /// </summary>
        [MarshalAs(UnmanagedType.R4)]
        public float Y;
        /// <summary>
        /// Z
        /// </summary>
        [MarshalAs(UnmanagedType.R4)]
        public float Z;
        /// <summary>
        /// W
        /// </summary>
        [MarshalAs(UnmanagedType.R4)]
        public float W;

    };

    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public struct CDeviceSampleInput
    {
        /// The X/Y/Z position of the device in 3D space
        public float PositionX;
        public float PositionY;
        public float PositionZ;

        /// A quaternion representing the orientation of the device in 3D space
        public float OrientationX;
        public float OrientationY;
        public float OrientationZ;
        public float OrientationW;

        /// The linear velocity of the device
        public float LinearVelocityX;
        public float LinearVelocityY;
        public float LinearVelocityZ;

        /// The linear acceleration of the device
        public float LinearAccelerationX;
        public float LinearAccelerationY;
        public float LinearAccelerationZ;

        /// The angular velocity of the device
        public float AngularVelocityX;
        public float AngularVelocityY;
        public float AngularVelocityZ;

        /// The angular acceleration of the device
        public float AngularAccelerationX;
        public float AngularAccelerationY;
        public float AngularAccelerationZ;
    };


    public struct CDeviceSample
    {
        /// The X/Y/Z position of the device in 3D space
        public Vector3 Position;

        /// A quaternion representing the orientation of the device in 3D space
        public Quat Orientation;

        /// The linear velocity of the device
        public Vector3 LinearVelocity;

        /// The linear acceleration of the device
        public Vector3 LinearAcceleration;

        /// The angular velocity of the device
        public Vector3 AngularVelocity;

        /// The angular acceleration of the device
        public Vector3 AngularAcceleration;
    };
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public struct CResults
    {
        [MarshalAs(UnmanagedType.R4)]
        public float Calories;
        [MarshalAs(UnmanagedType.R4)]
        public float HmdDistanceTravelled;
        [MarshalAs(UnmanagedType.R4)]
        public float LeftDistanceTravelled;
        [MarshalAs(UnmanagedType.R4)]
        public float RightDistanceTravelled;
        [MarshalAs(UnmanagedType.R4)]
        public float EstHeartRate;
        [MarshalAs(UnmanagedType.R4)]
        public float BurnRate;
        [MarshalAs(UnmanagedType.R4)]
        public float Squats;
        [MarshalAs(UnmanagedType.R8)]
        public double Timestamp;
        [MarshalAs(UnmanagedType.R8)]
        public double LinuxTimestamp;
        [MarshalAs(UnmanagedType.I4)]
        public ActivityLevel eActivityLevel;
        [MarshalAs(UnmanagedType.U4)]
        public uint m_nStepLeft;
        [MarshalAs(UnmanagedType.U4)]
        public uint m_nStepRight;
        [MarshalAs(UnmanagedType.U4)]
        public uint m_nJump;
        [MarshalAs(UnmanagedType.R8)]
        public double m_fLastStepTime;
        [MarshalAs(UnmanagedType.R8)]
        public double m_fMediumStepTime;
        [MarshalAs(UnmanagedType.R8)]
        public double m_fStepAccel;
    };
    [StructLayout(LayoutKind.Sequential, Pack = 1)]

    public struct StoredCalcDataInput
    {
        /// The X/Y/Z position of the device in 3D space
        [MarshalAs(UnmanagedType.R4)]
        public float m_HMD_PositionX;
        [MarshalAs(UnmanagedType.R4)]
        public float m_HMD_PositionY;
        [MarshalAs(UnmanagedType.R4)]
        public float m_HMD_PositionZ;

        /// A quaternion representing the orientation of the device in 3D space
        [MarshalAs(UnmanagedType.R4)]
        public float m_HMD_OrientationX;
        [MarshalAs(UnmanagedType.R4)]
        public float m_HMD_OrientationY;
        [MarshalAs(UnmanagedType.R4)]
        public float m_HMD_OrientationZ;
        [MarshalAs(UnmanagedType.R4)]
        public float m_HMD_OrientationW;

        /// The linear velocity of the device
        [MarshalAs(UnmanagedType.R4)]
        public float m_HMD_LinearVelocityX;
        [MarshalAs(UnmanagedType.R4)]
        public float m_HMD_LinearVelocityY;
        [MarshalAs(UnmanagedType.R4)]
        public float m_HMD_LinearVelocityZ;

        /// The linear acceleration of the device
        [MarshalAs(UnmanagedType.R4)]
        public float m_HMD_LinearAccelerationX;
        [MarshalAs(UnmanagedType.R4)]
        public float m_HMD_LinearAccelerationY;
        [MarshalAs(UnmanagedType.R4)]
        public float m_HMD_LinearAccelerationZ;

        /// The angular velocity of the device
        [MarshalAs(UnmanagedType.R4)]
        public float m_HMD_AngularVelocityX;
        [MarshalAs(UnmanagedType.R4)]
        public float m_HMD_AngularVelocityY;
        [MarshalAs(UnmanagedType.R4)]
        public float m_HMD_AngularVelocityZ;

        /// The angular acceleration of the device
        [MarshalAs(UnmanagedType.R4)]
        public float m_HMD_AngularAccelerationX;
        [MarshalAs(UnmanagedType.R4)]
        public float m_HMD_AngularAccelerationY;
        [MarshalAs(UnmanagedType.R4)]
        public float m_HMD_AngularAccelerationZ;


        /// The X/Y/Z position of the device in 3D space
        [MarshalAs(UnmanagedType.R4)]
        public float m_LeftController_PositionX;
        [MarshalAs(UnmanagedType.R4)]
        public float m_LeftController_PositionY;
        [MarshalAs(UnmanagedType.R4)]
        public float m_LeftController_PositionZ;

        /// A quaternion representing the orientation of the device in 3D space
        [MarshalAs(UnmanagedType.R4)]
        public float m_LeftController_OrientationX;
        [MarshalAs(UnmanagedType.R4)]
        public float m_LeftController_OrientationY;
        [MarshalAs(UnmanagedType.R4)]
        public float m_LeftController_OrientationZ;
        [MarshalAs(UnmanagedType.R4)]
        public float m_LeftController_OrientationW;

        /// The linear velocity of the device
        [MarshalAs(UnmanagedType.R4)]
        public float m_LeftController_LinearVelocityX;
        [MarshalAs(UnmanagedType.R4)]
        public float m_LeftController_LinearVelocityY;
        [MarshalAs(UnmanagedType.R4)]
        public float m_LeftController_LinearVelocityZ;

        /// The linear acceleration of the device
        [MarshalAs(UnmanagedType.R4)]
        public float m_LeftController_LinearAccelerationX;
        [MarshalAs(UnmanagedType.R4)]
        public float m_LeftController_LinearAccelerationY;
        [MarshalAs(UnmanagedType.R4)]
        public float m_LeftController_LinearAccelerationZ;

        /// The angular velocity of the device
        [MarshalAs(UnmanagedType.R4)]
        public float m_LeftController_AngularVelocityX;
        [MarshalAs(UnmanagedType.R4)]
        public float m_LeftController_AngularVelocityY;
        [MarshalAs(UnmanagedType.R4)]
        public float m_LeftController_AngularVelocityZ;

        /// The angular acceleration of the device
        [MarshalAs(UnmanagedType.R4)]
        public float m_LeftController_AngularAccelerationX;
        [MarshalAs(UnmanagedType.R4)]
        public float m_LeftController_AngularAccelerationY;
        [MarshalAs(UnmanagedType.R4)]
        public float m_LeftController_AngularAccelerationZ;



        /// The X/Y/Z position of the device in 3D space
        [MarshalAs(UnmanagedType.R4)]
        public float m_RightController_PositionX;
        [MarshalAs(UnmanagedType.R4)]
        public float m_RightController_PositionY;
        [MarshalAs(UnmanagedType.R4)]
        public float m_RightController_PositionZ;

        /// A quaternion representing the orientation of the device in 3D space
        [MarshalAs(UnmanagedType.R4)]
        public float m_RightController_OrientationX;
        [MarshalAs(UnmanagedType.R4)]
        public float m_RightController_OrientationY;
        [MarshalAs(UnmanagedType.R4)]
        public float m_RightController_OrientationZ;
        [MarshalAs(UnmanagedType.R4)]
        public float m_RightController_OrientationW;

        /// The linear velocity of the device
        [MarshalAs(UnmanagedType.R4)]
        public float m_RightController_LinearVelocityX;
        [MarshalAs(UnmanagedType.R4)]
        public float m_RightController_LinearVelocityY;
        [MarshalAs(UnmanagedType.R4)]
        public float m_RightController_LinearVelocityZ;

        /// The linear acceleration of the device
        [MarshalAs(UnmanagedType.R4)]
        public float m_RightController_LinearAccelerationX;
        [MarshalAs(UnmanagedType.R4)]
        public float m_RightController_LinearAccelerationY;
        [MarshalAs(UnmanagedType.R4)]
        public float m_RightController_LinearAccelerationZ;

        /// The angular velocity of the device
        [MarshalAs(UnmanagedType.R4)]
        public float m_RightController_AngularVelocityX;
        [MarshalAs(UnmanagedType.R4)]
        public float m_RightController_AngularVelocityY;
        [MarshalAs(UnmanagedType.R4)]
        public float m_RightController_AngularVelocityZ;

        /// The angular acceleration of the device
        [MarshalAs(UnmanagedType.R4)]
        public float m_RightController_AngularAccelerationX;
        [MarshalAs(UnmanagedType.R4)]
        public float m_RightController_AngularAccelerationY;
        [MarshalAs(UnmanagedType.R4)]
        public float m_RightController_AngularAccelerationZ;

        [MarshalAs(UnmanagedType.R4)]
        public float Calories;
        [MarshalAs(UnmanagedType.R4)]
        public float HmdDistanceTravelled;
        [MarshalAs(UnmanagedType.R4)]
        public float LeftDistanceTravelled;
        [MarshalAs(UnmanagedType.R4)]
        public float RightDistanceTravelled;
        [MarshalAs(UnmanagedType.R4)]
        public float EstHeartRate;
        [MarshalAs(UnmanagedType.R4)]
        public float Squats;
        [MarshalAs(UnmanagedType.R8)]
        public double Timestamp;
        [MarshalAs(UnmanagedType.I4)]
        public ActivityLevel eActivityLevel;
        [MarshalAs(UnmanagedType.U4)]
        public uint m_nStepLeft;
        [MarshalAs(UnmanagedType.U4)]
        public uint m_nStepRight;
        [MarshalAs(UnmanagedType.U4)]
        public uint m_nJump;
        [MarshalAs(UnmanagedType.R8)]
        double m_fLastStepTime;
        [MarshalAs(UnmanagedType.R8)]
        double m_fMediumStepTime;
        [MarshalAs(UnmanagedType.R8)]
        double m_fStepAccel;
        [MarshalAs(UnmanagedType.R8)]
        double m_fTimestamp;
    };
    public struct StoredCalcData
    {
        CDeviceSample m_HMD;
        CDeviceSample m_LeftController;
        CDeviceSample m_RightController;
        CResults m_Result;
        double m_fTimestamp;
    };


    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public class WorkoutActivity
    {
        [MarshalAs(UnmanagedType.R4)]
        public float Calories;
        [MarshalAs(UnmanagedType.R4)]
        public float Squats;
        [MarshalAs(UnmanagedType.R4)]
        public float EstHeartRate;
        [MarshalAs(UnmanagedType.R4)]
        public float LeftDistanceTravelled;
        [MarshalAs(UnmanagedType.R4)]
        public float RightDistanceTravelled;
        [MarshalAs(UnmanagedType.R4)]
        public float HmdDistanceTravelled;
        [MarshalAs(UnmanagedType.R4)]
        public float BurnRate;
        [MarshalAs(UnmanagedType.R8)]
        public double Timestamp;
    }




    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public class EventData
    {
        public enum TypeEvent
        {
            tOnServerCode,
            tOnDeviceCode,
            tOnInitialTokens,
            tOnRefreshResponse,
            tOnBioInformation,
            tOnProfileJSON,
            tOnLevelsJSON,
            tOnPreferencesJSON,
            tOnGetWidgetdataJSON,
            tOnStartWorkoutJSON,
            tOnUpdateWorkoutJSON,
            tOnFinalizeWorkoutJSON,
            tOnError,
            tOnSummariesJSON,
            tOnTestUserIDToken,
            tOnGetToken,
            tOnCheckEID,
            tOnRemoveID
        };
        public EventData()
        {
            typeEvent = TypeEvent.tOnError;
            m_pData = IntPtr.Zero;
            m_nSize = 0;
        }
        /// TypeEvent
        /// 
        public TypeEvent typeEvent;
        public IntPtr m_pData;
        public UInt32 m_nSize;
    }
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public struct ShortCodeResponseInternal
    {
        [MarshalAs(UnmanagedType.LPStr)]
        public IntPtr virtualptr;
        [MarshalAs(UnmanagedType.LPStr)]
        public IntPtr shortcode;
        [MarshalAs(UnmanagedType.LPStr)]
        public IntPtr devicecode;
        [MarshalAs(UnmanagedType.LPStr)]
        public IntPtr verification_url;
    };
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public struct ShortCodeResponseInternalCallBack
    {
        [MarshalAs(UnmanagedType.U4)]
        public UInt32 shortcode;
        [MarshalAs(UnmanagedType.U4)]
        public UInt32 devicecode;
        [MarshalAs(UnmanagedType.U4)]
        public UInt32 verification_url;
    };
    public class ShortCodeResponse
    {
        public string shortcode;
        public string devicecode;
        public string verification_url;
    };
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public class RefreshResponseInternal
    {
        [MarshalAs(UnmanagedType.LPStr)]
        public IntPtr virtualptr;
        [MarshalAs(UnmanagedType.LPStr)]
        public IntPtr refreshToken;
        [MarshalAs(UnmanagedType.LPStr)]
        public IntPtr idToken;
    };
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public class RefreshResponseInternalCallBack
    {
        [MarshalAs(UnmanagedType.U4)]
        public UInt32 refreshToken;
        [MarshalAs(UnmanagedType.U4)]
        public UInt32 idToken;
    };


    public class RefreshResponse
    {
        public string refreshToken;
        public string idToken;
    };

    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public class BioResponse
    {
        [MarshalAs(UnmanagedType.U4)]
        public uint weightKG;
        [MarshalAs(UnmanagedType.U4)]
        public uint heightCM;
        [MarshalAs(UnmanagedType.U4)]
        public uint age;
        [MarshalAs(UnmanagedType.U4)]
        public Sex sex;
    };

    public delegate void CallbackTypeYurNet(EventData value);
    public class Events
    {

        public static Action<ShortCodeResponse> OnServerCodeEvent = null;
        public static Action<string> OnDeviceCodeEvent = null;
        public static Action<RefreshResponse> OnInitialTokensEvent = null;
        public static Action<RefreshResponse> OnRefreshResponseEvent = null;
        public static Action<uint, uint, uint, Sex> OnBioInformationEvent = null;
        public static Action<string> OnProfileJSONEvent = null;
        public static Action<string> OnLevelsJSONEvent = null;
        public static Action<string> OnPreferencesJSONEvent = null;
        public static Action<string> OnGetWidgetdataJSONEvent = null;
        public static Action<string> OnStartWorkoutJSONEvent = null;
        public static Action<string> OnUpdateWorkoutJSONEvent = null;
        public static Action<string> OnFinalizeWorkoutJSONEvent = null;
        public static Action<string> OnErrorEvent = null;
        public static Action<string> OnSummariesJSONEvent = null;
        public static Action<string> OnTestUserIDTokenEvent = null;
        public static Action<string> OnGetTokenEvent = null;
        public static Action<string> OnCheckEIDEvent = null;
        public static Action<string> OnRemoveIDEvent = null;



        [AOT.MonoPInvokeCallback(typeof(CallbackTypeYurNet))]
        public static void OnServerCode(EventData value)
        {
            ShortCodeResponseInternalCallBack Results = new ShortCodeResponseInternalCallBack();
            GCHandle pResults = GCHandle.Alloc(Results, GCHandleType.Pinned);
            Import.MemCpy(pResults.AddrOfPinnedObject(), value.m_pData, (UInt32)Marshal.SizeOf(Results));
            Results = (ShortCodeResponseInternalCallBack)pResults.Target;

            byte[] Data = new byte[value.m_nSize];
            GCHandle pData = GCHandle.Alloc(Data, GCHandleType.Pinned);
            Import.MemCpy(pData.AddrOfPinnedObject(), value.m_pData, value.m_nSize);
            Data = (byte[])pData.Target;

            IntPtr baseAddr = pData.AddrOfPinnedObject();
            UInt64 baseAddr64 = (UInt64)baseAddr;

            ShortCodeResponse ret = new ShortCodeResponse();
            ret.shortcode = Marshal.PtrToStringAnsi((IntPtr)(baseAddr64 + Results.shortcode));
            ret.devicecode = Marshal.PtrToStringAnsi((IntPtr)(baseAddr64 + Results.devicecode));
            ret.verification_url = Marshal.PtrToStringAnsi((IntPtr)(baseAddr64 + Results.verification_url));

            //Import.FreeMem(value.m_pData);

            pResults.Free();
            pData.Free();

            if (OnServerCodeEvent != null)
                OnServerCodeEvent(ret);
        }
        [AOT.MonoPInvokeCallback(typeof(CallbackTypeYurNet))]
        public static void OnDeviceCode(EventData value)
        {
            if (OnDeviceCodeEvent != null)
                OnDeviceCodeEvent(Marshal.PtrToStringAnsi(value.m_pData));
            //Import.FreeMem(value.m_pData);
        }
        [AOT.MonoPInvokeCallback(typeof(CallbackTypeYurNet))]
        public static void OnInitialTokens(EventData value)
        {
            RefreshResponseInternalCallBack Results = new RefreshResponseInternalCallBack();
            GCHandle pResults = GCHandle.Alloc(Results, GCHandleType.Pinned);
            Import.MemCpy(pResults.AddrOfPinnedObject(), value.m_pData, (UInt32)Marshal.SizeOf(Results));
            Results = (RefreshResponseInternalCallBack)pResults.Target;

            byte[] Data = new byte[value.m_nSize];
            GCHandle pData = GCHandle.Alloc(Data, GCHandleType.Pinned);
            Import.MemCpy(pData.AddrOfPinnedObject(), value.m_pData, value.m_nSize);
            Data = (byte[])pData.Target;

            IntPtr baseAddr = pData.AddrOfPinnedObject();
            UInt64 baseAddr64 = (UInt64)baseAddr;

            RefreshResponse ret = new RefreshResponse();
            ret.idToken = Marshal.PtrToStringAnsi((IntPtr)(baseAddr64 + Results.idToken));
            ret.refreshToken = Marshal.PtrToStringAnsi((IntPtr)(baseAddr64 + Results.refreshToken));
            //Import.FreeMem(value.m_pData);

            pResults.Free();
            pData.Free();

            if (OnInitialTokensEvent != null)
                OnInitialTokensEvent(ret);
        }
        [AOT.MonoPInvokeCallback(typeof(CallbackTypeYurNet))]
        public static void OnRefreshResponse(EventData value)
        {
            RefreshResponseInternalCallBack Results = new RefreshResponseInternalCallBack();
            GCHandle pResults = GCHandle.Alloc(Results, GCHandleType.Pinned);
            Import.MemCpy(pResults.AddrOfPinnedObject(), value.m_pData, (UInt32)Marshal.SizeOf(Results));
            Results = (RefreshResponseInternalCallBack)pResults.Target;

            byte[] Data = new byte[value.m_nSize];
            GCHandle pData = GCHandle.Alloc(Data, GCHandleType.Pinned);
            Import.MemCpy(pData.AddrOfPinnedObject(), value.m_pData, value.m_nSize);
            Data = (byte[])pData.Target;

            IntPtr baseAddr = pData.AddrOfPinnedObject();
            UInt64 baseAddr64 = (UInt64)baseAddr;

            RefreshResponse ret = new RefreshResponse();
            ret.idToken = Marshal.PtrToStringAnsi((IntPtr)(baseAddr64 + Results.idToken));
            ret.refreshToken = Marshal.PtrToStringAnsi((IntPtr)(baseAddr64 + Results.refreshToken));
            //Import.FreeMem(value.m_pData);
            pResults.Free();
            pData.Free();

            if (OnRefreshResponseEvent != null)
                OnRefreshResponseEvent(ret);
        }
        [AOT.MonoPInvokeCallback(typeof(CallbackTypeYurNet))]
        public static void OnBioInformation(EventData value)
        {
            BioResponse Results = new BioResponse();
            GCHandle pResults = GCHandle.Alloc(Results, GCHandleType.Pinned);
            Import.MemCpy(pResults.AddrOfPinnedObject(), value.m_pData, value.m_nSize);
            Results = (BioResponse)pResults.Target;
            pResults.Free();
            //Import.FreeMem(value.m_pData);
            if (OnBioInformationEvent != null)
                OnBioInformationEvent(Results.weightKG, Results.heightCM, Results.age, Results.sex);
        }
        [AOT.MonoPInvokeCallback(typeof(CallbackTypeYurNet))]
        public static void OnProfileJSON(EventData value)
        {
            if (OnProfileJSONEvent != null)
                OnProfileJSONEvent(Marshal.PtrToStringAnsi(value.m_pData));
            //Import.FreeMem(value.m_pData);
        }

        [AOT.MonoPInvokeCallback(typeof(CallbackTypeYurNet))]
        public static void OnLevelsJSON(EventData value)
        {
            if (OnLevelsJSONEvent != null)
                OnLevelsJSONEvent(Marshal.PtrToStringAnsi(value.m_pData));
            //Import.FreeMem(value.m_pData);
        }
        [AOT.MonoPInvokeCallback(typeof(CallbackTypeYurNet))]
        public static void OnPreferencesJSON(EventData value)
        {
            if (OnPreferencesJSONEvent != null)
                OnPreferencesJSONEvent(Marshal.PtrToStringAnsi(value.m_pData));
            //Import.FreeMem(value.m_pData);
        }
        [AOT.MonoPInvokeCallback(typeof(CallbackTypeYurNet))]
        public static void OnGetWidgetdataJSON(EventData value)
        {
            if (OnGetWidgetdataJSONEvent != null)
                OnGetWidgetdataJSONEvent(Marshal.PtrToStringAnsi(value.m_pData));
            //Import.FreeMem(value.m_pData);
        }
        [AOT.MonoPInvokeCallback(typeof(CallbackTypeYurNet))]
        public static void OnStartWorkoutJSON(EventData value)
        {
            if (OnStartWorkoutJSONEvent != null)
                OnStartWorkoutJSONEvent(Marshal.PtrToStringAnsi(value.m_pData));
            //Import.FreeMem(value.m_pData);
        }
        [AOT.MonoPInvokeCallback(typeof(CallbackTypeYurNet))]
        public static void OnUpdateWorkoutJSON(EventData value)
        {
            if (OnUpdateWorkoutJSONEvent != null)
                OnUpdateWorkoutJSONEvent(Marshal.PtrToStringAnsi(value.m_pData));
            //Import.FreeMem(value.m_pData);
        }
        [AOT.MonoPInvokeCallback(typeof(CallbackTypeYurNet))]
        public static void OnFinalizeWorkoutJSON(EventData value)
        {
            if (OnFinalizeWorkoutJSONEvent != null)
                OnFinalizeWorkoutJSONEvent(Marshal.PtrToStringAnsi(value.m_pData));
            //Import.FreeMem(value.m_pData);
        }
        [AOT.MonoPInvokeCallback(typeof(CallbackTypeYurNet))]
        public static void OnError(EventData value)
        {
            if (OnErrorEvent != null)
                OnErrorEvent(Marshal.PtrToStringAnsi(value.m_pData));
            //Import.FreeMem(value.m_pData);
        }

        public static void OnSummariesJSON(EventData value)
        {
            if (OnSummariesJSONEvent != null)
                OnSummariesJSONEvent(Marshal.PtrToStringAnsi(value.m_pData));
        }
        public static void OnTestUserIDToken(EventData value)
        {
            if (OnTestUserIDTokenEvent != null)
                OnTestUserIDTokenEvent(Marshal.PtrToStringAnsi(value.m_pData));
        }
        public static void OnGetToken(EventData value)
        {
            if (OnGetTokenEvent != null)
                OnGetTokenEvent(Marshal.PtrToStringAnsi(value.m_pData));
        }
        public static void OnCheckEID(EventData value)
        {
            if (OnCheckEIDEvent != null)
                OnCheckEIDEvent(Marshal.PtrToStringAnsi(value.m_pData));
        }
        public static void OnRemoveID(EventData value)
        {
            if (OnRemoveIDEvent != null)
                OnRemoveIDEvent(Marshal.PtrToStringAnsi(value.m_pData));
        }

        public void Dispose()
        {
            OnServerCodeEvent = null;
            OnDeviceCodeEvent = null;
            OnInitialTokensEvent = null;
            OnRefreshResponseEvent = null;
            OnBioInformationEvent = null;
            OnProfileJSONEvent = null;
            OnLevelsJSONEvent = null;
            OnPreferencesJSONEvent = null;
            OnGetWidgetdataJSONEvent = null;
            OnStartWorkoutJSONEvent = null;
            OnUpdateWorkoutJSONEvent = null;
            OnFinalizeWorkoutJSONEvent = null;
            OnErrorEvent = null;
            OnSummariesJSONEvent = null;
            OnTestUserIDTokenEvent = null;
            OnGetTokenEvent = null;
            OnCheckEIDEvent = null;
            OnRemoveIDEvent = null;
        }
    }


    private static class Import
    {
        [DllImport("YUR_SDK_UNITY")]
        public static extern bool Init();
        [DllImport("YUR_SDK_UNITY")]
        public static extern void Calculator(IntPtr pHMD, IntPtr pLeftController, IntPtr pRightController, IntPtr pResults, double dTimestamp = 0.0);
        [DllImport("YUR_SDK_UNITY")]
        public static extern void CalculatorAsync(IntPtr pHMD, IntPtr pLeftController, IntPtr pRightController,  double dTimestamp=0.0);
        [DllImport("YUR_SDK_UNITY")]
        public static extern void GetAsyncResult( IntPtr pResults);
        [DllImport("YUR_SDK_UNITY")]

        public static extern void SetBioData(int age, int sex, float weightKG, float heightCM, float restingHeartRate);
        [DllImport("YUR_SDK_UNITY")]
        public static extern float CalcCaloriesFromHeartRate(float fHR, float fDeltaTime);
        [DllImport("YUR_SDK_UNITY")]
        public static extern void ResetCalculationState();
        [DllImport("YUR_SDK_UNITY")]
        public static extern void ClearCurrentCounter();

        [DllImport("YUR_SDK_UNITY")]
        public static extern void ResetStepsCalculation();

        [DllImport("YUR_SDK_UNITY")]
        public static extern bool CalcDataAction(IntPtr sPath, StoreCalcData nCode, CalcDataFormat nFormat);
        [DllImport("YUR_SDK_UNITY")]
        public static extern void GetCalcData(int nIndex, IntPtr pRet);
        [DllImport("YUR_SDK_UNITY")]
        public static extern int GetCalcDataSize();
        [DllImport("YUR_SDK_UNITY")]
        public static extern void RecalcVelocitysAndAccelerations();

        [DllImport("YUR_SDK_UNITY")]
        public static extern void SetGameInfo(IntPtr sGameLicence, IntPtr sGameName, IntPtr sVersion, SubPlatoform pt);

        [DllImport("YUR_SDK_UNITY")]
        public static extern bool DisconnectToken();
        [DllImport("YUR_SDK_UNITY")]
        public static extern void FreeMem(IntPtr pData);


        [DllImport("YUR_SDK_UNITY")]
        public static extern bool GetServerCode(IntPtr pShortCodeResponse);
        [DllImport("YUR_SDK_UNITY")]
        public static extern IntPtr SendDeviceCode(IntPtr shortcode, IntPtr devicecode);
        [DllImport("YUR_SDK_UNITY")]
        public static extern bool GetInitialTokens(IntPtr sDeviceKey, IntPtr pReturn);
        [DllImport("YUR_SDK_UNITY")]
        public static extern bool UseRefreshToken(IntPtr sDeviceKey, IntPtr pReturn);
        [DllImport("YUR_SDK_UNITY")]
        public static extern IntPtr GetProfileJSON(IntPtr sToken);
        [DllImport("YUR_SDK_UNITY")]
        public static extern bool GetBioInformations(IntPtr sToken, IntPtr pBioInfo);
        [DllImport("YUR_SDK_UNITY")]
        public static extern IntPtr GetLevels(IntPtr sToken);
        [DllImport("YUR_SDK_UNITY")]
        public static extern IntPtr GetPreferences(IntPtr sToken);
        [DllImport("YUR_SDK_UNITY")]
        public static extern IntPtr GetWidgetdata(IntPtr sToken);
        [DllImport("YUR_SDK_UNITY")]
        public static extern IntPtr StartWorkout(IntPtr sToken);
        [DllImport("YUR_SDK_UNITY")]
        public static extern IntPtr UpdateWorkout(IntPtr sToken, IntPtr workoutID, IntPtr activity, UInt32 nActivityLen);
        [DllImport("YUR_SDK_UNITY")]
        public static extern IntPtr FinalizeWorkout(IntPtr sToken, IntPtr workoutID);


        [DllImport("YUR_SDK_UNITY")]
        public static extern void GetServerCodeAsync();
        [DllImport("YUR_SDK_UNITY")]
        public static extern void SendDeviceCodeAsync(IntPtr shortcode, IntPtr devicecode);
        [DllImport("YUR_SDK_UNITY")]
        public static extern void GetInitialTokensAsync(IntPtr sDeviceKey);
        [DllImport("YUR_SDK_UNITY")]
        public static extern void UseRefreshTokenAsync(IntPtr sDeviceKey);
        [DllImport("YUR_SDK_UNITY")]
        public static extern void GetBioInformationsAsync(IntPtr sToken);
        [DllImport("YUR_SDK_UNITY")]
        public static extern void GetProfileAsync(IntPtr sToken);
        [DllImport("YUR_SDK_UNITY")]
        public static extern void GetLevelsAsync(IntPtr sToken);
        [DllImport("YUR_SDK_UNITY")]
        public static extern void GetPreferencesAsync(IntPtr sToken);
        [DllImport("YUR_SDK_UNITY")]
        public static extern void GetWidgetdataAsync(IntPtr sToken);
        [DllImport("YUR_SDK_UNITY")]
        public static extern void StartWorkoutAsync(IntPtr sToken);
        [DllImport("YUR_SDK_UNITY")]
        public static extern void UpdateWorkoutAsync(IntPtr sToken, IntPtr workoutID, IntPtr activity, UInt32 nActivityLen);
        [DllImport("YUR_SDK_UNITY")]
        public static extern void FinalizeWorkoutAsync(IntPtr sToken, IntPtr workoutID);
        [DllImport("YUR_SDK_UNITY")]
        public static extern IntPtr GetEventMessage();
        [DllImport("YUR_SDK_UNITY")]
        public static extern void MemCpy(IntPtr pDest, IntPtr pSrc, UInt32 nSize);

        [DllImport("YUR_SDK_UNITY")]
        public static extern void SetTag(IntPtr sTag);

        [DllImport("YUR_SDK_UNITY")]
        public static extern bool IsOnline();

        [DllImport("YUR_SDK_UNITY")]
        public static extern void InitRootPath(IntPtr sPublicPath, IntPtr sPrivatePath);

        [DllImport("YUR_SDK_UNITY")]
        public static extern IntPtr GetToken();

        [DllImport("YUR_SDK_UNITY")]
        public static extern bool BridgeStartWorkout();
        [DllImport("YUR_SDK_UNITY")]
        public static extern bool BridgeUpdateWorkout();
        [DllImport("YUR_SDK_UNITY")]
        public static extern bool BridgeFinalizeWorkout();

        [DllImport("YUR_SDK_UNITY")]
        public static extern void StartNetwork(IntPtr sPrivatePath, IntPtr sUserID,IntPtr sPublicPath);

    };

    public class CYurFitSDK
    {

        public static readonly uint _YUR_FIT_SDK_VERSION = 0x0100;

        private static bool initialized = false;

        public bool Init()
        {
            ResetCalculationState();
            ResetStepsCalculation();
            initialized = true;
            return Import.Init();
        }


        /// Integrate tracking sample for cumulative calculation.  <see cref="AddSample" /> should be called once per frame.
        public CResults Calculator(CDeviceSample HMD, CDeviceSample LeftController, CDeviceSample RightController, double fTimestamp = 0.0)
        {

            CDeviceSampleInput lHmd = new CDeviceSampleInput();

            lHmd.PositionX = HMD.Position.X;
            lHmd.PositionY = HMD.Position.Y;
            lHmd.PositionZ = HMD.Position.Z;

            lHmd.OrientationX = HMD.Orientation.X;
            lHmd.OrientationY = HMD.Orientation.Y;
            lHmd.OrientationZ = HMD.Orientation.Z;
            lHmd.OrientationW = HMD.Orientation.W;

            lHmd.LinearVelocityX = HMD.LinearVelocity.X;
            lHmd.LinearVelocityY = HMD.LinearVelocity.Y;
            lHmd.LinearVelocityZ = HMD.LinearVelocity.Z;

            lHmd.LinearAccelerationX = HMD.LinearAcceleration.X;
            lHmd.LinearAccelerationY = HMD.LinearAcceleration.Y;
            lHmd.LinearAccelerationZ = HMD.LinearAcceleration.Z;

            lHmd.AngularVelocityX = HMD.AngularVelocity.X;
            lHmd.AngularVelocityY = HMD.AngularVelocity.Y;
            lHmd.AngularVelocityZ = HMD.AngularVelocity.Z;

            lHmd.AngularAccelerationX = HMD.AngularAcceleration.X;
            lHmd.AngularAccelerationY = HMD.AngularAcceleration.Y;
            lHmd.AngularAccelerationZ = HMD.AngularAcceleration.Z;

            GCHandle pHMD = GCHandle.Alloc(lHmd, GCHandleType.Pinned);

            CDeviceSampleInput lLeftController = new CDeviceSampleInput();

            lLeftController.PositionX = LeftController.Position.X;
            lLeftController.PositionY = LeftController.Position.Y;
            lLeftController.PositionZ = LeftController.Position.Z;

            lLeftController.OrientationX = LeftController.Orientation.X;
            lLeftController.OrientationY = LeftController.Orientation.Y;
            lLeftController.OrientationZ = LeftController.Orientation.Z;
            lLeftController.OrientationW = LeftController.Orientation.W;

            lLeftController.LinearVelocityX = LeftController.LinearVelocity.X;
            lLeftController.LinearVelocityY = LeftController.LinearVelocity.Y;
            lLeftController.LinearVelocityZ = LeftController.LinearVelocity.Z;

            lLeftController.LinearAccelerationX = LeftController.LinearAcceleration.X;
            lLeftController.LinearAccelerationY = LeftController.LinearAcceleration.Y;
            lLeftController.LinearAccelerationZ = LeftController.LinearAcceleration.Z;

            lLeftController.AngularVelocityX = LeftController.AngularVelocity.X;
            lLeftController.AngularVelocityY = LeftController.AngularVelocity.Y;
            lLeftController.AngularVelocityZ = LeftController.AngularVelocity.Z;

            lLeftController.AngularAccelerationX = LeftController.AngularAcceleration.X;
            lLeftController.AngularAccelerationY = LeftController.AngularAcceleration.Y;
            lLeftController.AngularAccelerationZ = LeftController.AngularAcceleration.Z;

            GCHandle pLeftController = GCHandle.Alloc(lLeftController, GCHandleType.Pinned);

            CDeviceSampleInput lRightController = new CDeviceSampleInput();

            lRightController.PositionX = RightController.Position.X;
            lRightController.PositionY = RightController.Position.Y;
            lRightController.PositionZ = RightController.Position.Z;

            lRightController.OrientationX = RightController.Orientation.X;
            lRightController.OrientationY = RightController.Orientation.Y;
            lRightController.OrientationZ = RightController.Orientation.Z;
            lRightController.OrientationW = RightController.Orientation.W;

            lRightController.LinearVelocityX = RightController.LinearVelocity.X;
            lRightController.LinearVelocityY = RightController.LinearVelocity.Y;
            lRightController.LinearVelocityZ = RightController.LinearVelocity.Z;

            lRightController.LinearAccelerationX = RightController.LinearAcceleration.X;
            lRightController.LinearAccelerationY = RightController.LinearAcceleration.Y;
            lRightController.LinearAccelerationZ = RightController.LinearAcceleration.Z;

            lRightController.AngularVelocityX = RightController.AngularVelocity.X;
            lRightController.AngularVelocityY = RightController.AngularVelocity.Y;
            lRightController.AngularVelocityZ = RightController.AngularVelocity.Z;

            lRightController.AngularAccelerationX = RightController.AngularAcceleration.X;
            lRightController.AngularAccelerationY = RightController.AngularAcceleration.Y;
            lRightController.AngularAccelerationZ = RightController.AngularAcceleration.Z;

            GCHandle pRightController = GCHandle.Alloc(lRightController, GCHandleType.Pinned);
            CResults Results = new CResults();
            GCHandle pResults = GCHandle.Alloc(Results, GCHandleType.Pinned);

            Import.Calculator(pHMD.AddrOfPinnedObject(), pLeftController.AddrOfPinnedObject(), pRightController.AddrOfPinnedObject(), pResults.AddrOfPinnedObject(), fTimestamp);

            Results = (CResults)pResults.Target;
            pResults.Free();

            pHMD.Free();
            pLeftController.Free();
            pRightController.Free();
            return Results;
        }

        public void CalculatorAsync(CDeviceSample HMD, CDeviceSample LeftController, CDeviceSample RightController, double fTimestamp = 0.0)
        {

            CDeviceSampleInput lHmd = new CDeviceSampleInput();

            lHmd.PositionX = HMD.Position.X;
            lHmd.PositionY = HMD.Position.Y;
            lHmd.PositionZ = HMD.Position.Z;

            lHmd.OrientationX = HMD.Orientation.X;
            lHmd.OrientationY = HMD.Orientation.Y;
            lHmd.OrientationZ = HMD.Orientation.Z;
            lHmd.OrientationW = HMD.Orientation.W;

            lHmd.LinearVelocityX = HMD.LinearVelocity.X;
            lHmd.LinearVelocityY = HMD.LinearVelocity.Y;
            lHmd.LinearVelocityZ = HMD.LinearVelocity.Z;

            lHmd.LinearAccelerationX = HMD.LinearAcceleration.X;
            lHmd.LinearAccelerationY = HMD.LinearAcceleration.Y;
            lHmd.LinearAccelerationZ = HMD.LinearAcceleration.Z;

            lHmd.AngularVelocityX = HMD.AngularVelocity.X;
            lHmd.AngularVelocityY = HMD.AngularVelocity.Y;
            lHmd.AngularVelocityZ = HMD.AngularVelocity.Z;

            lHmd.AngularAccelerationX = HMD.AngularAcceleration.X;
            lHmd.AngularAccelerationY = HMD.AngularAcceleration.Y;
            lHmd.AngularAccelerationZ = HMD.AngularAcceleration.Z;

            GCHandle pHMD = GCHandle.Alloc(lHmd, GCHandleType.Pinned);

            CDeviceSampleInput lLeftController = new CDeviceSampleInput();

            lLeftController.PositionX = LeftController.Position.X;
            lLeftController.PositionY = LeftController.Position.Y;
            lLeftController.PositionZ = LeftController.Position.Z;

            lLeftController.OrientationX = LeftController.Orientation.X;
            lLeftController.OrientationY = LeftController.Orientation.Y;
            lLeftController.OrientationZ = LeftController.Orientation.Z;
            lLeftController.OrientationW = LeftController.Orientation.W;

            lLeftController.LinearVelocityX = LeftController.LinearVelocity.X;
            lLeftController.LinearVelocityY = LeftController.LinearVelocity.Y;
            lLeftController.LinearVelocityZ = LeftController.LinearVelocity.Z;

            lLeftController.LinearAccelerationX = LeftController.LinearAcceleration.X;
            lLeftController.LinearAccelerationY = LeftController.LinearAcceleration.Y;
            lLeftController.LinearAccelerationZ = LeftController.LinearAcceleration.Z;

            lLeftController.AngularVelocityX = LeftController.AngularVelocity.X;
            lLeftController.AngularVelocityY = LeftController.AngularVelocity.Y;
            lLeftController.AngularVelocityZ = LeftController.AngularVelocity.Z;

            lLeftController.AngularAccelerationX = LeftController.AngularAcceleration.X;
            lLeftController.AngularAccelerationY = LeftController.AngularAcceleration.Y;
            lLeftController.AngularAccelerationZ = LeftController.AngularAcceleration.Z;

            GCHandle pLeftController = GCHandle.Alloc(lLeftController, GCHandleType.Pinned);

            CDeviceSampleInput lRightController = new CDeviceSampleInput();

            lRightController.PositionX = RightController.Position.X;
            lRightController.PositionY = RightController.Position.Y;
            lRightController.PositionZ = RightController.Position.Z;

            lRightController.OrientationX = RightController.Orientation.X;
            lRightController.OrientationY = RightController.Orientation.Y;
            lRightController.OrientationZ = RightController.Orientation.Z;
            lRightController.OrientationW = RightController.Orientation.W;

            lRightController.LinearVelocityX = RightController.LinearVelocity.X;
            lRightController.LinearVelocityY = RightController.LinearVelocity.Y;
            lRightController.LinearVelocityZ = RightController.LinearVelocity.Z;

            lRightController.LinearAccelerationX = RightController.LinearAcceleration.X;
            lRightController.LinearAccelerationY = RightController.LinearAcceleration.Y;
            lRightController.LinearAccelerationZ = RightController.LinearAcceleration.Z;

            lRightController.AngularVelocityX = RightController.AngularVelocity.X;
            lRightController.AngularVelocityY = RightController.AngularVelocity.Y;
            lRightController.AngularVelocityZ = RightController.AngularVelocity.Z;

            lRightController.AngularAccelerationX = RightController.AngularAcceleration.X;
            lRightController.AngularAccelerationY = RightController.AngularAcceleration.Y;
            lRightController.AngularAccelerationZ = RightController.AngularAcceleration.Z;

            GCHandle pRightController = GCHandle.Alloc(lRightController, GCHandleType.Pinned);

            Import.CalculatorAsync(pHMD.AddrOfPinnedObject(), pLeftController.AddrOfPinnedObject(), pRightController.AddrOfPinnedObject(), fTimestamp);

            pHMD.Free();
            pLeftController.Free();
            pRightController.Free();
        }
        public CResults GetAsyncResult()
        {


            CResults Results = new CResults();
            GCHandle pResults = GCHandle.Alloc(Results, GCHandleType.Pinned);

            Import.GetAsyncResult( pResults.AddrOfPinnedObject());

            Results = (CResults)pResults.Target;
            pResults.Free();
            return Results;
        }


        /// <summary>
        /// Sets the biometric data for the current player.  Needs to be called prior to calling <see cref="AddSample"/>
        /// </summary>
        /// <param name="age">The player's age in years</param>
        /// <param name="sex">The player's sex</param>
        /// <param name="weightKG">The player's weight in kilograms</param>
        /// <param name="heightCM">The player's height in centimeters</param>
        /// <param name="restingHeartRate">Optionally, the player's resting heart rate, if known</param>
        public void SetBioData(int age, Sex sex, float weightKG, float heightCM, float restingHeartRate = 75)
        {
            Import.SetBioData(age, (int)sex, weightKG, heightCM, restingHeartRate);
        }


        public float CalcCaloriesFromHeartRate(float fHR, float fDeltaTime)
        {
            return Import.CalcCaloriesFromHeartRate(fHR, fDeltaTime);
        }

        /// Resets the internal calculation state returning the calculator to a "cold start".
        /// Does NOT clear the current activity counter.
        public void ResetCalculationState()
        {
            Import.ResetCalculationState();
        }

        /// Clears the <see cref="CurrentCounter"/> cumulative counter for calories, distance travelled and squats back to zero
        public void ClearCurrentCounter()
        {
            Import.ClearCurrentCounter();
        }
        public void ResetStepsCalculation()
        {
            Import.ResetStepsCalculation();
        }

        private byte[] StrToCPtr(string sText)
        {
            return System.Text.Encoding.ASCII.GetBytes(sText + "\0");
        }

        public bool CalcDataAction(string sPath, StoreCalcData nCode, CalcDataFormat nFormat = CalcDataFormat.Json)
        {
            GCHandle pPath = GCHandle.Alloc(StrToCPtr(sPath), GCHandleType.Pinned);
            bool bRet = Import.CalcDataAction(pPath.AddrOfPinnedObject(), nCode, nFormat);
            pPath.Free();
            return bRet;
        }

        public StoredCalcDataInput GetCalcData(int nIndex)
        {
            StoredCalcDataInput ret = new StoredCalcDataInput();

            GCHandle pRet = GCHandle.Alloc(ret, GCHandleType.Pinned);

            Import.GetCalcData(nIndex, pRet.AddrOfPinnedObject());

            pRet.Free();

            return ret;
        }

        public int GetCalcDataSize()
        {
            return Import.GetCalcDataSize();
        }

        public void SetGameInfo(string sGameLicence, string sGameName, string sVersion, SubPlatoform pt)
        {
            GCHandle pGameLicence = GCHandle.Alloc(StrToCPtr(sGameLicence), GCHandleType.Pinned);
            GCHandle pGameName = GCHandle.Alloc(StrToCPtr(sGameName), GCHandleType.Pinned);
            GCHandle pVersion = GCHandle.Alloc(StrToCPtr(sVersion), GCHandleType.Pinned);

            Import.SetGameInfo(pGameLicence.AddrOfPinnedObject(), pGameName.AddrOfPinnedObject(), pVersion.AddrOfPinnedObject(),pt);

            pGameLicence.Free();
            pGameName.Free();
            pVersion.Free();
        }

        public void InitRootPath(string sPublicPath, string sPrivatePath)
        {
            GCHandle pPublicPath  = GCHandle.Alloc(StrToCPtr(sPublicPath),  GCHandleType.Pinned);
            GCHandle pPrivatePath = GCHandle.Alloc(StrToCPtr(sPrivatePath), GCHandleType.Pinned);
            Import.InitRootPath(pPublicPath.AddrOfPinnedObject(), pPrivatePath.AddrOfPinnedObject());
            pPublicPath.Free();
            pPrivatePath.Free();
        }
        public string GetToken()
        {
            IntPtr ret = Import.GetToken();
            if (ret != IntPtr.Zero)
            {
                string sRet = Marshal.PtrToStringAnsi(ret);
                // Import.FreeMem(ret);
                return sRet;
            }
            return "";
        }

        public bool DisconnectToken()
        {
            bool bRet;
            bRet = Import.DisconnectToken();
            return bRet;
        }

        public ShortCodeResponse GetServerCode()
        {
            ShortCodeResponseInternal Results = new ShortCodeResponseInternal();
            GCHandle pResults = GCHandle.Alloc(Results, GCHandleType.Pinned);
            if (Import.GetServerCode(pResults.AddrOfPinnedObject()))
            {
                ShortCodeResponse ret = new ShortCodeResponse();
                Results = (ShortCodeResponseInternal)pResults.Target;
                ret.shortcode = Marshal.PtrToStringAnsi(Results.shortcode);
                ret.devicecode = Marshal.PtrToStringAnsi(Results.devicecode);
                ret.verification_url = Marshal.PtrToStringAnsi(Results.verification_url);
                pResults.Free();
                return ret;
            }
            pResults.Free();
            return null;
        }
        public string SendDeviceCode(string shortcode, string devicecode)
        {
            GCHandle pShortcode = GCHandle.Alloc(StrToCPtr(shortcode), GCHandleType.Pinned);
            GCHandle pDevicecode = GCHandle.Alloc(StrToCPtr(devicecode), GCHandleType.Pinned);

            IntPtr ret = Import.SendDeviceCode(pShortcode.AddrOfPinnedObject(), pDevicecode.AddrOfPinnedObject());
            pShortcode.Free();
            pDevicecode.Free();
            if (ret != IntPtr.Zero)
            {
                string sRet = Marshal.PtrToStringAnsi(ret);
                Import.FreeMem(ret);
                return sRet;
            }
            return "";
        }
        public RefreshResponse GetInitialTokens(string sDeviceKey)
        {
            GCHandle pDeviceKey = GCHandle.Alloc(StrToCPtr(sDeviceKey), GCHandleType.Pinned);

            RefreshResponseInternal Results = new RefreshResponseInternal();
            GCHandle pResults = GCHandle.Alloc(Results, GCHandleType.Pinned);

            if (Import.GetInitialTokens(pDeviceKey.AddrOfPinnedObject(), pResults.AddrOfPinnedObject()))
            {
                pDeviceKey.Free();
                Results = (RefreshResponseInternal)pResults.Target;
                pResults.Free();

                RefreshResponse ret = new RefreshResponse();

                ret.idToken = Marshal.PtrToStringAnsi(Results.idToken);
                ret.refreshToken = Marshal.PtrToStringAnsi(Results.refreshToken);

                return ret;
            }
            pResults.Free();
            return null;
        }
        public RefreshResponse UseRefreshToken(string sDeviceKey)
        {
            GCHandle pDeviceKey = GCHandle.Alloc(StrToCPtr(sDeviceKey), GCHandleType.Pinned);

            RefreshResponseInternal Results = new RefreshResponseInternal();
            GCHandle pResults = GCHandle.Alloc(Results, GCHandleType.Pinned);

            if (Import.UseRefreshToken(pDeviceKey.AddrOfPinnedObject(), pResults.AddrOfPinnedObject()))
            {
                pDeviceKey.Free();
                Results = (RefreshResponseInternal)pResults.Target;
                pResults.Free();

                RefreshResponse ret = new RefreshResponse();

                ret.idToken = Marshal.PtrToStringAnsi(Results.idToken);
                ret.refreshToken = Marshal.PtrToStringAnsi(Results.refreshToken);

                return ret;
            }
            pResults.Free();
            return null;
        }
        public string GetProfileJSON(string sToken)
        {
            GCHandle pToken = GCHandle.Alloc(StrToCPtr(sToken), GCHandleType.Pinned);

            IntPtr ret = Import.GetProfileJSON(pToken.AddrOfPinnedObject());
            pToken.Free();
            if (ret != IntPtr.Zero)
            {
                string sRet = Marshal.PtrToStringAnsi(ret);
                Import.FreeMem(ret);
                return sRet;
            }
            return "";
        }

        public BioResponse GetBioInformations(string sToken)
        {
            BioResponse Results = new BioResponse();
            GCHandle pResults = GCHandle.Alloc(Results, GCHandleType.Pinned);
            GCHandle pToken = GCHandle.Alloc(StrToCPtr(sToken), GCHandleType.Pinned);
            bool bRet = Import.GetBioInformations(pToken.AddrOfPinnedObject(), pResults.AddrOfPinnedObject());
            Results = (BioResponse)pResults.Target;
            pResults.Free();
            pToken.Free();

            if (bRet)
            {
                return Results;
            }
            else
            {
                return null;
            }
        }

        public string GetLevels(string sToken)
        {
            GCHandle pToken = GCHandle.Alloc(StrToCPtr(sToken), GCHandleType.Pinned);

            IntPtr ret = Import.GetLevels(pToken.AddrOfPinnedObject());
            pToken.Free();
            if (ret != IntPtr.Zero)
            {
                string sRet = Marshal.PtrToStringAnsi(ret);
                Import.FreeMem(ret);
                return sRet;
            }
            return "";
        }
        public string GetPreferences(string sToken)
        {
            GCHandle pToken = GCHandle.Alloc(StrToCPtr(sToken), GCHandleType.Pinned);

            IntPtr ret = Import.GetPreferences(pToken.AddrOfPinnedObject());
            pToken.Free();
            if (ret != IntPtr.Zero)
            {
                string sRet = Marshal.PtrToStringAnsi(ret);
                Import.FreeMem(ret);
                return sRet;
            }
            return "";
        }
        public string GetWidgetdata(string sToken)
        {
            GCHandle pToken = GCHandle.Alloc(StrToCPtr(sToken), GCHandleType.Pinned);

            IntPtr ret = Import.GetWidgetdata(pToken.AddrOfPinnedObject());
            pToken.Free();
            if (ret != IntPtr.Zero)
            {
                string sRet = Marshal.PtrToStringAnsi(ret);
                Import.FreeMem(ret);
                return sRet;
            }
            return "";
        }
        public string StartWorkout(string sToken)
        {
            GCHandle pToken = GCHandle.Alloc(StrToCPtr(sToken), GCHandleType.Pinned);
            IntPtr ret = Import.StartWorkout(pToken.AddrOfPinnedObject());
            pToken.Free();
            if (ret != IntPtr.Zero)
            {
                string sRet = Marshal.PtrToStringAnsi(ret);
                Import.FreeMem(ret);
                return sRet;
            }
            return "";
        }
        public string UpdateWorkout(string sToken, string sWorkoutID, WorkoutActivity[] Activity)
        {
            GCHandle pToken = GCHandle.Alloc(StrToCPtr(sToken), GCHandleType.Pinned);
            GCHandle pWorkoutID = GCHandle.Alloc(StrToCPtr(sWorkoutID), GCHandleType.Pinned);
            GCHandle pActivity = GCHandle.Alloc(Activity, GCHandleType.Pinned);

            IntPtr ret = Import.UpdateWorkout(pToken.AddrOfPinnedObject(), pWorkoutID.AddrOfPinnedObject(), pActivity.AddrOfPinnedObject(), (UInt32)Activity.Length);

            pActivity.Free();
            pToken.Free();
            pWorkoutID.Free();
            if (ret != IntPtr.Zero)
            {
                string sRet = Marshal.PtrToStringAnsi(ret);
                Import.FreeMem(ret);
                return sRet;
            }
            return "";
        }
        public string FinalizeWorkout(string sToken, string sWorkoutID)
        {
            GCHandle pToken = GCHandle.Alloc(StrToCPtr(sToken), GCHandleType.Pinned);
            GCHandle pWorkoutID = GCHandle.Alloc(StrToCPtr(sWorkoutID), GCHandleType.Pinned);
            IntPtr ret = Import.FinalizeWorkout(pToken.AddrOfPinnedObject(), pWorkoutID.AddrOfPinnedObject());
            pToken.Free();
            pWorkoutID.Free();
            if (ret != IntPtr.Zero)
            {
                string sRet = Marshal.PtrToStringAnsi(ret);
                Import.FreeMem(ret);
                return sRet;
            }
            return "";
        }

        public EventData GetEventMessage()
        {
            IntPtr eventMsg = Import.GetEventMessage();
            EventData eventData = null;
            if (eventMsg != IntPtr.Zero)
            {
                eventData = new EventData();
                GCHandle pRetPhysicalMaxValue = GCHandle.Alloc(eventData, GCHandleType.Pinned);
                Import.MemCpy(pRetPhysicalMaxValue.AddrOfPinnedObject(), eventMsg, (UInt32)Marshal.SizeOf(eventData));
                pRetPhysicalMaxValue.Free();
            }
            return eventData;
        }

        public void Update()
        {
            if (!initialized)
            {
                Import.Init();
            }

            EventData eventData = GetEventMessage();
            if (eventData != null)
            {
                switch (eventData.typeEvent)
                {
                    case EventData.TypeEvent.tOnServerCode:
                        Events.OnServerCode(eventData);
                        break;
                    case EventData.TypeEvent.tOnDeviceCode:
                        Events.OnDeviceCode(eventData);
                        break;
                    case EventData.TypeEvent.tOnInitialTokens:
                        Events.OnInitialTokens(eventData);
                        break;
                    case EventData.TypeEvent.tOnRefreshResponse:
                        Events.OnRefreshResponse(eventData);
                        break;
                    case EventData.TypeEvent.tOnBioInformation:
                        Events.OnBioInformation(eventData);
                        break;
                    case EventData.TypeEvent.tOnProfileJSON:
                        Events.OnProfileJSON(eventData);
                        break;
                    case EventData.TypeEvent.tOnLevelsJSON:
                        Events.OnLevelsJSON(eventData);
                        break;
                    case EventData.TypeEvent.tOnPreferencesJSON:
                        Events.OnPreferencesJSON(eventData);
                        break;
                    case EventData.TypeEvent.tOnGetWidgetdataJSON:
                        Events.OnGetWidgetdataJSON(eventData);
                        break;
                    case EventData.TypeEvent.tOnStartWorkoutJSON:
                        Events.OnStartWorkoutJSON(eventData);
                        break;
                    case EventData.TypeEvent.tOnUpdateWorkoutJSON:
                        Events.OnUpdateWorkoutJSON(eventData);
                        break;
                    case EventData.TypeEvent.tOnFinalizeWorkoutJSON:
                        Events.OnFinalizeWorkoutJSON(eventData);
                        break;
                    case EventData.TypeEvent.tOnError:
                        Events.OnError(eventData);
                        break;
                    case EventData.TypeEvent.tOnSummariesJSON:
						Events.OnSummariesJSON(eventData);
                        break;
                    case EventData.TypeEvent.tOnTestUserIDToken:
						Events.OnTestUserIDToken(eventData);
                        break;
                    case EventData.TypeEvent.tOnGetToken:
						Events.OnGetToken(eventData);
                        break;
                    case EventData.TypeEvent.tOnCheckEID:
						Events.OnCheckEID(eventData);
                        break;
                    case EventData.TypeEvent.tOnRemoveID:
						Events.OnRemoveID(eventData);					
                        break;
                }
            }
        }
        public void GetServerCodeAsync()
        {
            Import.GetServerCodeAsync();
        }

        public void SendDeviceCodeAsync(string shortcode, string devicecode)
        {
            GCHandle pShortcode = GCHandle.Alloc(StrToCPtr(shortcode), GCHandleType.Pinned);
            GCHandle pDevicecode = GCHandle.Alloc(StrToCPtr(devicecode), GCHandleType.Pinned);

            Import.SendDeviceCodeAsync(pShortcode.AddrOfPinnedObject(), pDevicecode.AddrOfPinnedObject());
            pShortcode.Free();
            pDevicecode.Free();
        }
        public void GetInitialTokensAsync(string sDeviceKey)
        {
            GCHandle pDeviceKey = GCHandle.Alloc(StrToCPtr(sDeviceKey), GCHandleType.Pinned);

            Import.GetInitialTokensAsync(pDeviceKey.AddrOfPinnedObject());
            pDeviceKey.Free();
        }
        public void UseRefreshTokenAsync(string sDeviceKey)
        {
            GCHandle pDeviceKey = GCHandle.Alloc(StrToCPtr(sDeviceKey), GCHandleType.Pinned);

            Import.UseRefreshTokenAsync(pDeviceKey.AddrOfPinnedObject());
            pDeviceKey.Free();
        }
        public void GetProfileAsync(string sToken)
        {
            GCHandle pToken = GCHandle.Alloc(StrToCPtr(sToken), GCHandleType.Pinned);

            Import.GetProfileAsync(pToken.AddrOfPinnedObject());
            pToken.Free();
        }

        public void GetBioInformationsAsync(string sToken)
        {
            GCHandle pToken = GCHandle.Alloc(StrToCPtr(sToken), GCHandleType.Pinned);
            Import.GetBioInformationsAsync(pToken.AddrOfPinnedObject());
            pToken.Free();
        }
        public void GetLevelsAsync(string sToken)
        {
            GCHandle pToken = GCHandle.Alloc(StrToCPtr(sToken), GCHandleType.Pinned);
            Import.GetLevelsAsync(pToken.AddrOfPinnedObject());
            pToken.Free();
        }
        public void GetPreferencesAsync(string sToken)
        {
            GCHandle pToken = GCHandle.Alloc(StrToCPtr(sToken), GCHandleType.Pinned);
            Import.GetPreferencesAsync(pToken.AddrOfPinnedObject());
            pToken.Free();
        }
        public void GetWidgetdataAsync(string sToken)
        {
            GCHandle pToken = GCHandle.Alloc(StrToCPtr(sToken), GCHandleType.Pinned);
            Import.GetWidgetdataAsync(pToken.AddrOfPinnedObject());
            pToken.Free();
        }
        public void StartWorkoutAsync(string sToken)
        {
            GCHandle pToken = GCHandle.Alloc(StrToCPtr(sToken), GCHandleType.Pinned);
            Import.StartWorkoutAsync(pToken.AddrOfPinnedObject());
            pToken.Free();
        }

        public void UpdateWorkoutAsync(string sToken, string sWorkoutID, WorkoutActivity[] Activity)
        {
            GCHandle pToken = GCHandle.Alloc(StrToCPtr(sToken), GCHandleType.Pinned);
            GCHandle pWorkoutID = GCHandle.Alloc(StrToCPtr(sWorkoutID), GCHandleType.Pinned);
            GCHandle pActivity = GCHandle.Alloc(Activity, GCHandleType.Pinned);
            Import.UpdateWorkoutAsync(pToken.AddrOfPinnedObject(), pWorkoutID.AddrOfPinnedObject(), pActivity.AddrOfPinnedObject(), (UInt32)Activity.Length);
            pActivity.Free();
            pToken.Free();
            pWorkoutID.Free();
        }
        public void FinalizeWorkoutAsync(string sToken, string sWorkoutID)
        {
            GCHandle pToken = GCHandle.Alloc(StrToCPtr(sToken), GCHandleType.Pinned);
            GCHandle pWorkoutID = GCHandle.Alloc(StrToCPtr(sWorkoutID), GCHandleType.Pinned);
            Import.FinalizeWorkoutAsync(pToken.AddrOfPinnedObject(), pWorkoutID.AddrOfPinnedObject());
            pToken.Free();
            pWorkoutID.Free();
        }
        public void SetTag(string sTag)
        {
            GCHandle pTag = GCHandle.Alloc(StrToCPtr(sTag), GCHandleType.Pinned);
            Import.SetTag(pTag.AddrOfPinnedObject());
            pTag.Free();
        }

        public bool IsOnline()
        {
            return Import.IsOnline();
        }
        public bool BridgeStartWorkout()
        {
            return Import.BridgeStartWorkout();
        }
        public bool BridgeUpdateWorkout()
        {
            return Import.BridgeUpdateWorkout();
        }
        public bool BridgeFinalizeWorkout()
        {
            return Import.BridgeFinalizeWorkout();
        }
        public void StartNetwork(string sPrivatePath, string sUserID,string sPublicPath)
        {
            GCHandle pPrivatePath = GCHandle.Alloc(StrToCPtr(sPrivatePath), GCHandleType.Pinned);
            GCHandle pPublicPath = new GCHandle();
            if (sPublicPath.Length!=0)
            {
                pPublicPath = GCHandle.Alloc(StrToCPtr(sPublicPath), GCHandleType.Pinned);
            }

            if (sUserID.Length == 0)
            {
                Import.StartNetwork(pPrivatePath.AddrOfPinnedObject(), IntPtr.Zero, pPublicPath.AddrOfPinnedObject());
            }
            else
            {
                GCHandle pUserID = GCHandle.Alloc(StrToCPtr(sUserID), GCHandleType.Pinned);
                Import.StartNetwork(pPrivatePath.AddrOfPinnedObject(), pUserID.AddrOfPinnedObject(), IntPtr.Zero);
                pUserID.Free();
            }
            pPrivatePath.Free();
        }
    };
}


