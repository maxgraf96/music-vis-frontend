using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using System.Runtime.InteropServices;
using UnityEngine;

public class Main : MonoBehaviour
{
    [DllImport("mapper", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.StdCall)]
    public static extern IntPtr mapper_version();

    [DllImport("mapper", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.StdCall)]
    public static extern IntPtr mapper_device_new([MarshalAs(UnmanagedType.LPStr)] string devname, int port, int net);

    [DllImport("mapper", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.StdCall)]
    public static extern IntPtr mapper_device_add_output_signal(IntPtr dev,
        [MarshalAs(UnmanagedType.LPStr)] string name,
        int length,
        char type,
        int min,
        int max);

    [DllImport("mapper", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.StdCall)]
    public static extern IntPtr mapper_device_add_input_signal(IntPtr dev,
        [MarshalAs(UnmanagedType.LPStr)] string name,
        int length,
        char type,
        string unit,
        Int32 min,
        Int32 max,
        Func<IntPtr, IntPtr, IntPtr, Int32, IntPtr, IntPtr> handler,
        IntPtr userData
        );

    [DllImport("mapper", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.StdCall)]
    public static extern int mapper_device_ready(IntPtr dev);

    [DllImport("mapper", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.StdCall)]
    public static extern int mapper_device_poll(IntPtr dev, int block_ms);

    [DllImport("mapper", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.StdCall)]
    public static extern int mapper_device_poll(IntPtr dev);

    [DllImport("mapper", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.StdCall)]
    public static extern void mapper_signal_update_float(IntPtr sig, float val);

    IntPtr dev;
    static float currentValue = 0.0f;

    private static IntPtr updateTest(IntPtr sig, IntPtr instance, IntPtr value, Int32 count, IntPtr timetag)
    {
        unsafe
        {
            float val = *(float*)value;
            if (val != currentValue)
            {
                currentValue = val;
                print(currentValue);
            }
        }

        return IntPtr.Zero;
    }

    void Start()
    {
        IntPtr intPtr = mapper_version();
        string recv_str = "mapper version = " + System.Runtime.InteropServices.Marshal.PtrToStringAnsi(intPtr);
        print(recv_str);

        dev = mapper_device_new("/csmapper", 0, 0);
        print("created device /csmapper");

        IntPtr sig = mapper_device_add_input_signal(dev, "testsig1", 1, 'f', "", 0, 0, updateTest, IntPtr.Zero);
        print("created input testsig1");

        // Initialise device
        print("Waiting for device");
        while (mapper_device_ready(dev) == 0)
        {
            mapper_device_poll(dev, 25);
        }
        print("Device ready");
    }

    void Update()
    {
        mapper_device_poll(dev, 25);

        transform.position = new Vector3(transform.position.x, currentValue * 0.01f * Time.deltaTime, 0);
    }
}
