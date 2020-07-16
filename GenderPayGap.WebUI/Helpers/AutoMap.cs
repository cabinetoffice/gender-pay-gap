using System;
using Autofac;
using AutoMapper;
using GenderPayGap.Core;
using GenderPayGap.WebUI;

namespace GenderPayGap.Extensions
{
    public static class AutoMap
    {

        [Obsolete("We are trying to reduce the use of Automapper. Please don't use this")]
        public static S GetClone<S>(this S source)
        {
            if (source == null || source.Equals(default(S)))
            {
                return default;
            }

            var mapperConfig = new MapperConfiguration(
                cfg =>
                {
                    // allows auto mapper to inject our dependencies
                    cfg.ConstructServicesUsing(type => Global.ContainerIoC.Resolve(type));
                    // register all out mapper profiles (classes/mappers/*)
                    cfg.AddMaps(typeof(Program).Assembly);

                    cfg.CreateMap<S, S>();
                });

            IMapper iMapper = mapperConfig.CreateMapper();
            return iMapper.Map<S, S>(source);
        }

    }
}
