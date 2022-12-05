using DemoServer.Models;
using DemoServer.Services.acars;

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
                Payload = "Test Downlink, Please ignore.",
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
            var downlink = new AcarsDownlink(request);
            downlink.DataSize = downlink.Payload.Length;
            downlink.Id = Guid.NewGuid();
            downlink.TimeStamp = DateTime.UtcNow;
            downlink.State = AcarsDownlinkState.queued;
            downlink.StatusUpateTimeStamp = DateTime.UtcNow;
            PublishDownlinkToClientHandlers(downlink);
            Downlinks.Add(downlink);
            Task.Run(() => DownlinkEmulation(downlink.Id));
            return downlink;
            
        }

        public void AddUplink()
        {
            uplinkIdCounter++;
            var uplink = new AcarsUplink()
            {
                Id = Guid.NewGuid(),
                Mti = "AB",
                DataType = AcarsDataType.ASCII,
                //TimeStamp = DateTime.UtcNow.ToString("yyyy-MM-ddTHH:mm:ss:fff"),
                TimeStamp = DateTime.UtcNow

            };
            uplink.Payload = "EDDE 051700Z 0518/0618 25006KT 3500 BR BKN015 BECMG 0519/0523 BKN001 TEMPO 0520/0602 0700 FG BECMG 0601/0605 8000 BKN015 TEMPO 0605/0618 3500 -RASN BKN010";
            uplink.DataSize = uplink.Payload.Length;
            
            Uplinks.Add(uplink);
            PublishUplinkToClientHander(uplink);
        }

        public void DeleteUplinks()
        {
            foreach (var item in Uplinks)
            {
                item.State = AcarsUplinkState.deleted;
                PublishUplinkToClientHander(item);
            }
            Uplinks.Clear();
        }

        public void DeleteDownlinks()
        {
            foreach (var item in Downlinks)
            {
                item.State = AcarsDownlinkState.deleted;
                item.StatusUpateTimeStamp = DateTime.UtcNow;
                PublishDownlinkToClientHandlers(item);
            }
            Downlinks.Clear();
        }

        public bool DeleteDownlink(Guid id)
        {
            var itemToRemove = Downlinks.SingleOrDefault(dl => dl.Id == id);
            if (itemToRemove != null)
            {
                itemToRemove.State = AcarsDownlinkState.deleted;
                itemToRemove.StatusUpateTimeStamp = DateTime.UtcNow;
                PublishDownlinkToClientHandlers(itemToRemove);
                Downlinks.Remove(itemToRemove);
                return true;
            }
            return false;
        }

        public bool DeleteUplink(Guid id)
        {
            var itemToRemove = Uplinks.SingleOrDefault(ul => ul.Id == id);
            if (itemToRemove != null)
            {
                itemToRemove.State = AcarsUplinkState.deleted;                
                PublishUplinkToClientHander(itemToRemove);
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

        private void PublishDownlinkToClientHandlers(object msg)
        {
            foreach (var client in _clients)
            {
                client.ReceiveDownlinkUpdate(msg);
            }
        }

        private void PublishUplinkToClientHander(object msg)
        {
            foreach (var  client in _clients)
            {
                client.ReceiveUplinkUpdate(msg);
            }
        }

        private async Task DownlinkEmulation(Guid id)
        {
            // Emulate a positive downlink
            await Task.Delay(2000);
            await UpdateDownlinkStatus(id, AcarsDownlinkState.cmf_transfer);            
            await Task.Delay(2000);
            await UpdateDownlinkStatus(id, AcarsDownlinkState.cmf_ack);
            await Task.Delay(2000);
            await UpdateDownlinkStatus(id, AcarsDownlinkState.dsp_ack);
        }

        private async Task UpdateDownlinkStatus(Guid id, AcarsDownlinkState state)
        {
            _logger.LogInformation($"Change Downlink status of ID: {id} to: {state}");
            var downlink = Downlinks.First(dl => dl.Id == id);
            downlink.State = state;
            downlink.StatusUpateTimeStamp = DateTime.UtcNow;
            PublishDownlinkToClientHandlers(downlink);
        }
    }
}
