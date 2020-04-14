//
// Copyright (c) Deutsche Lufthansa AG.
//
// This source code is licensed under the MIT license found in the
// LICENSE file in the root directory of this source tree.
//

using AutoMapper;
using DemoServer.DataAccess.Models;
using DemoServer.Models;

namespace DemoServer.MappingProfiles
{
    public class DemoServerProfile : Profile
    {
        public DemoServerProfile()
        {
            CreateMap<Message, MessageEntity>().ReverseMap();
        }
    }
}