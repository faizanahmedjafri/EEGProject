using System;
using Android.App;
using Android.Runtime;

namespace biopot.Droid
{
#if DEBUG    
    [Application(Debuggable = true, Label = "biopot", Icon = "@mipmap/ic_launch")]
#else
    [Application(Debuggable = false, Label = "biopot", Icon = "@mipmap/ic_launch")]
#endif
    public sealed class MainApplication : Application
    {
        public MainApplication(IntPtr handle, JniHandleOwnership transer) : base(handle, transer)
        {
        }
        public override void OnCreate()
        {
            base.OnCreate();
        }
    }
}
