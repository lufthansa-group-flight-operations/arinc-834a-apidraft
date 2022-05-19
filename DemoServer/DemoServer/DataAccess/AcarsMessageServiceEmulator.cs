using DemoServer.Models;
using DemoServer.WebSockets;

namespace DemoServer.DataAccess
{
    public class AcarsMessageServiceEmulator : IAcarsMessageService
    {
        private readonly ILogger<AcarsMessageServiceEmulator> _logger;
        private List<IWebSocketClientHandlerAcars> _clients;
        private int uplinkIdCounter, downlinkIdCounter = 0;

        public List<AcarsUplink> Uplinks { get; set; }
        public List<AcarsDownlink> Downlinks { get; set; }    


        public AcarsMessageServiceEmulator(ILogger<AcarsMessageServiceEmulator> logger)
        {
            _logger = logger;
            Uplinks = new List<AcarsUplink>();
            Downlinks = new List<AcarsDownlink>();
            _clients = new List<IWebSocketClientHandlerAcars>();

            // Fill up some data
            downlinkIdCounter++;
            SendDownlink(new AcarsDownlinkRequest()
            {
                Id = downlinkIdCounter,
                MediaSelect = AcarsMediaSelect.MEDIA_ANY,
                Data = "VGVzdGRvd25saW5rICMx",
                DataSize = 20,
                DataType = AcarsDataType.ASCII,
                LifeTime = 0,
            });

            AddUplink();
        }

        public AcarsStatus GetStatus()
        {
            return new AcarsStatus()
            {
                Updated = DateTime.UtcNow.ToString("yyyy-MM-ddTHH:mm:ss.fffZ"),
                IsAnyAvailable = true,
                Channels = new ChannelInfo[]
                {
                    new ChannelInfo(){Name = "VHF", IsAvailable = true },
                    new ChannelInfo(){Name = "HF", IsAvailable = false },
                    new ChannelInfo(){Name = "SATCOM", IsAvailable = true },
                    new ChannelInfo(){Name = "GATELINK", IsAvailable = false },
                    new ChannelInfo(){Name = "ANY_OTHER", IsAvailable = false }
                }
            };

        }

        public AcarsDownlink? SendDownlink(AcarsDownlinkRequest request)
        {
            downlinkIdCounter++;
            var dl = new AcarsDownlink(request);            
            dl.Id = downlinkIdCounter;
            dl.Created = DateTime.UtcNow.ToString("yyyy-mm-ddTHH:mm:ss:fffZ");
            dl.State = AcarsDownlinkState.WAITING;
            Downlinks.Add(dl);
            Task.Run(() => DownlinkEmulation(dl.Id));
            return dl;
            
        }

        public void AddUplink()
        {
            uplinkIdCounter++;
            var uplink = new AcarsUplink()
            {
                Id = uplinkIdCounter,
                Mfi = "AB",
                DataType = AcarsDataType.ASCII,
                TimeStamp = DateTime.UtcNow.ToString("yyyy-MM-ddTHH:mm:ss:fff"),
                
            };
            uplink.SetPlainTextToBas64($"Testuplink #{uplink.Id}");
            uplink.DataSize = uplink.Data.Length;
            
            Uplinks.Add(uplink);
            PublishToClientHandlers(uplink);
        }

        public void DeleteUplinks()
        {
            Uplinks.Clear();
        }

        public void DeleteDownlinks()
        {
            Downlinks.Clear();
        }

        public bool DeleteDownlink(int id)
        {
            var itemToRemove = Downlinks.SingleOrDefault(dl => dl.Id == id);
            if (itemToRemove != null)
            {
                Downlinks.Remove(itemToRemove);
                return true;
            }
            return false;
        }

        public bool DeleteUplink(int id)
        {
            var itemToRemove = Uplinks.SingleOrDefault(ul => ul.Id == id);
            if (itemToRemove != null)
            {
                Uplinks.Remove(itemToRemove);
                return true;
            }
            return false;
        }

        public void Subscribe(IWebSocketClientHandlerAcars client)
        {
            _logger.LogDebug("Add client to subscription list");
            _clients.Add(client);
        }

        public void Unsubscribe(IWebSocketClientHandlerAcars client)
        {
            if (_clients.Contains(client))
            {
                _logger.LogDebug("Remove client from subscription list");
                _clients.Remove(client);
            }
        }

        private void PublishToClientHandlers(object msg)
        {
            foreach (var client in _clients)
            {
                client.ReceiveDownlinkUpdate(msg);
            }
        }

        private async Task DownlinkEmulation(int id)
        {
            // Emulate a positive downlink
            await Task.Delay(2000);
            await UpdateDownlinkStatus(id, AcarsDownlinkState.TRANSMITTING);            
            await Task.Delay(2000);
            await UpdateDownlinkStatus(id, AcarsDownlinkState.SENT);
            await Task.Delay(2000);
            await UpdateDownlinkStatus(id, AcarsDownlinkState.ACKNOWLEDGED);
        }

        private async Task UpdateDownlinkStatus(int id, AcarsDownlinkState state)
        {
            _logger.LogInformation($"Change Downlink status of ID: {id} to: {state}");
            var downlink = Downlinks.First(dl => dl.Id == id);
            downlink.State = state;
            PublishToClientHandlers(downlink);
        }
    }
}
