using System;
using AutoMapper;

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

            var mapperConfig = new MapperConfiguration(cfg => { cfg.CreateMap<S, S>(); });
            IMapper iMapper = mapperConfig.CreateMapper();
            return iMapper.Map<S, S>(source);
        }

    }
}
