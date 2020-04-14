//
// Copyright (c) Deutsche Lufthansa AG.
//
// This source code is licensed under the MIT license found in the
// LICENSE file in the root directory of this source tree.
//

using AutoMapper;
using DemoServer.DataAccess.Models;
using DemoServer.MappingProfiles;
using DemoServer.Models;
using NUnit.Framework;

namespace DemoServerTests
{
    public class MapperTests
    {
        private readonly IMapper _mapper;

        public MapperTests()
        {
            var mapperConfiguration = new MapperConfiguration(cfg => { cfg.AddProfile<DemoServerProfile>(); });
            mapperConfiguration.AssertConfigurationIsValid();
            mapperConfiguration.CompileMappings();
            _mapper = mapperConfiguration.CreateMapper();
        }

        [SetUp]
        public void Setup()
        {
        }

        [Test]
        public void Map_Message_ModelToEntity()
        {
            var model = new Message();
            var entity = _mapper.Map<MessageEntity>(model);

            Assert.NotNull(entity);
        }

        [Test]
        public void Map_Message_EntityToModel()
        {
            var entity = new MessageEntity();
            var model = _mapper.Map<Message>(entity);

            Assert.NotNull(model);
        }
    }
}