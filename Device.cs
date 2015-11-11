﻿using System;
using System.Drawing;
using System.Runtime.InteropServices;

namespace Freenect2
{
	public class Device : IDisposable
	{
        private const int COLOR_WIDTH  = 1920;
        private const int COLOR_HEIGHT = 1080;

        private const int DEPTH_WIDTH  = 512;
        private const int DEPTH_HEIGHT = 424;
        private const float MAX_DEPTH  = 4500;

        private IntPtr handle;
        private FrameCallback frameCallback;

        private Int32[]  colorBuffer;
        private GCHandle colorBufferHandle;

        private Single[] depthBuffer;
        private GCHandle depthBufferHandle;

		public static int Count 
		{
			get { return freenect2_context_get_device_count(Context); }
		}

        public Size FrameSize { get { return new Size(COLOR_WIDTH, COLOR_HEIGHT); } }
        public float MaxDepth { get { return MAX_DEPTH; } }

        public event Action<Int32[], Single[]> FrameReceived;

		public Device(int id)
		{
            handle = freenect2_device_create(Context, id);

            if (handle == IntPtr.Zero) {
                throw new Exception("Could not create Kinect device");
            }

            frameCallback = new FrameCallback(HandleFrame);
            freenect2_device_set_frame_callback(handle, frameCallback);

            colorBuffer = new Int32[COLOR_WIDTH * COLOR_HEIGHT];
            colorBufferHandle = GCHandle.Alloc(colorBuffer, GCHandleType.Pinned);
            freenect2_device_set_color_buffer(handle, colorBufferHandle.AddrOfPinnedObject());

            depthBuffer = new Single[COLOR_WIDTH * COLOR_HEIGHT];
            depthBufferHandle = GCHandle.Alloc(depthBuffer, GCHandleType.Pinned);
            freenect2_device_set_depth_buffer(handle, depthBufferHandle.AddrOfPinnedObject());
		}

        public void Dispose()
        {
            if (handle != IntPtr.Zero) {
                freenect2_device_destroy(handle);
                handle = IntPtr.Zero;
            }

            colorBufferHandle.Free();
            depthBufferHandle.Free();
        }

        public void Start()
        {
            freenect2_device_start(handle);
        }

        public void Stop()
        {
            freenect2_device_stop(handle);
        }

        private void HandleFrame() {
            if (FrameReceived == null) return;
            FrameReceived(colorBuffer, depthBuffer);
        }

		#region Native
        private static IntPtr context;

        private static IntPtr Context
        {
            get
            {
                if (context == IntPtr.Zero) {
                    context = freenect2_context_create();
                }

                return context;
            }
        }

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        private delegate void FrameCallback();

        [DllImport("freenect2c")] private static extern IntPtr freenect2_context_create();
        [DllImport("freenect2c")] private static extern void   freenect2_context_destroy(IntPtr context);

		[DllImport("freenect2c")] private static extern int    freenect2_context_get_device_count(IntPtr context);
        [DllImport("freenect2c")] private static extern IntPtr freenect2_device_create(IntPtr context, int id);
        [DllImport("freenect2c")] private static extern IntPtr freenect2_device_destroy(IntPtr device);
        [DllImport("freenect2c")] private static extern IntPtr freenect2_device_start(IntPtr device);
        [DllImport("freenect2c")] private static extern IntPtr freenect2_device_stop(IntPtr device);
        [DllImport("freenect2c")] private static extern void   freenect2_device_set_frame_callback(IntPtr device, FrameCallback callback);
        [DllImport("freenect2c")] private static extern void   freenect2_device_set_color_buffer(IntPtr device, IntPtr buffer);
        [DllImport("freenect2c")] private static extern void   freenect2_device_set_depth_buffer(IntPtr device, IntPtr buffer);

		#endregion
	}
}

