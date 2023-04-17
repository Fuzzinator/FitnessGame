namespace YUR.Core {
    // static class exposing functions for YUR-related conversions, e.g. v3, quat
    public static class YURConversion
    {
        // convert a `UnityEngine.Vector3` to a `YUR_SDK.Vector3`
        public static YUR_SDK.Vector3 AsYURVector3(this UnityEngine.Vector3 u)
        {
            return new YUR_SDK.Vector3()
            {
                X = u.x,
                Y = u.y,
                Z = u.z
            };
        }

        // convert a `UnityEngine.Quaternion` to a `YUR_SDK.Quat`
        public static YUR_SDK.Quat AsYURQuaternion(this UnityEngine.Quaternion u)
        {
            return new YUR_SDK.Quat()
            {
                W = u.w,
                X = u.x,
                Y = u.y,
                Z = u.z
            };
        }

        public static YUR_SDK.CDeviceSample AsYURDeviceSample(this UnityEngine.Transform t)
        {
            YUR_SDK.CDeviceSample ds = new();
            ds.Position = AsYURVector3(t.position);
            ds.Orientation = AsYURQuaternion(t.rotation);
            return ds;
        }
    }
}
