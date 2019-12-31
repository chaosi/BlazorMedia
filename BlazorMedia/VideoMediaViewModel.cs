﻿using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using System.Linq;

namespace BlazorMedia
{
    public class VideoMediaViewModel : ComponentBase, IDisposable
    {
        [Inject]
        IJSRuntime JS { get; set; }

        bool IsInitialized { get; set; }
        protected ElementReference VideoElementRef { get; set; }
        [Parameter]
        public EventCallback<byte[]> OnDataReceived { get; set; }
        private int _timeslice = 0;
        [Parameter]
        public int Timeslice
        {
            get
            {
                return _timeslice;
            }
            set
            {
                if(_timeslice != value)
                {
                    _timeslice = value;
                    JS.InvokeAsync<dynamic>("BlazorMedia.BlazorMediaInterop.SetVideoRecorderTimeslice", VideoElementRef, value);
                }
            }
        }

        protected override async Task OnAfterRenderAsync(bool firstRender)
        {
            if(firstRender)
            {
                await InitializeComponentAsync();
            }

            await base.OnAfterRenderAsync(firstRender);
        }

        protected async Task InitializeComponentAsync()
        {
            await JS.InvokeAsync<dynamic>(
                "BlazorMedia.BlazorMediaInterop.InitializeVideoElement",
                VideoElementRef,
                DotNetObjectReference.Create(this),
                Timeslice);
        }

        [JSInvokable]
        public async void ReceiveDataAsync(int[] data)
        {
            /// @TODO: C# Blazor wont accept ArrayUint8 from JS so we pass the binary data as int[] and convert to byte[]
            byte[] buffer = data.Cast<int>().Select(i => (byte)i).ToArray();
            if (OnDataReceived.HasDelegate)
                await OnDataReceived.InvokeAsync(buffer);
        }

        public async void Dispose()
        {
            try
            {
                await JS.InvokeAsync<dynamic>(
                    "BlazorMedia.BlazorMediaInterop.DisposeVideoElement",
                    VideoElementRef);
            }
            catch 
            {
                // Page has been reloaded, API is not available
            }
        }
    }
}
