using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Q42.HueApi;
using Q42.HueApi.ColorConverters;
using Q42.HueApi.ColorConverters.HSB;
using Q42.HueApi.Interfaces;

namespace Assets.Scripts
{
    public class HueLightHelper
    {
        private ILocalHueClient client;
        private IEnumerable<Light> lights;
        private HueSettings hueSettings;


        public Action Connected;

        public bool IsConnected { get; set; }
        

        public HueLightHelper(HueSettings hueSettings)
        {
            this.hueSettings = hueSettings;
        }

        public async Task ChangeLight (string lightName, UnityEngine.Color color)
        {
            if (this.client == null)
            {
                return;
            }

            var lightToChange = this.lights.FirstOrDefault((l) => l.Name == lightName);
            if (lightToChange != null)
            {
                var command = new LightCommand();
                var lightColor = new RGBColor(color.r, color.g, color.b);
                command.TurnOn().SetColor(lightColor);

                await client.SendCommandAsync(command, new string[] { lightToChange.Id });
            }
        }

        public async Task TurnOff()
        {
            if (this.client != null)
            {
                var command = new LightCommand();
                command.TurnOff();
                await this.client.SendCommandAsync(command);
            }
        }

        public async Task Connect()
        {
            IBridgeLocator locator = new HttpBridgeLocator(); //Or: LocalNetworkScanBridgeLocator, MdnsBridgeLocator, MUdpBasedBridgeLocator
            var bridges = await locator.LocateBridgesAsync(TimeSpan.FromSeconds(5));

            if (bridges.Any())
            {
                var bridge = bridges.First();
                string ipAddressOfTheBridge = bridge.IpAddress;
                this.client = new LocalHueClient(ipAddressOfTheBridge);
                

                if (!string.IsNullOrEmpty(hueSettings.AppKey))
                {
                    this.client.Initialize(hueSettings.AppKey);
                }

                this.lights = await client.GetLightsAsync();
                IsConnected = true;
                Connected?.Invoke();
            }
        }

        public async Task RegisterAppWithHueBridge()
        {
            // TODO:Make sure the user has pressed the button on the bridge before calling RegisterAsync
            //It will throw an LinkButtonNotPressedException if the user did not press the button

            var appKey = await client.RegisterAsync(hueSettings.AppName, hueSettings.DeviceName);
            if (!string.IsNullOrEmpty(appKey))
            {
                hueSettings.AppKey = appKey;
            }
        }
    }
}
