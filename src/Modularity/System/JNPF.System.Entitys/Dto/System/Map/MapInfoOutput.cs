﻿using JNPF.Dependency;

namespace JNPF.System.Entitys.Dto.System.Map
{
    /// <summary>
    /// 地图明细
    /// </summary>
    [SuppressSniffer]
    public class MapInfoOutput
    {
        /// <summary>
        /// 主键id
        /// </summary>
        public string id { get; set; }

        /// <summary>
        /// 地图编码
        /// </summary>
        public string enCode { get; set; }

        /// <summary>
        /// 地图名称
        /// </summary>
        public string fullName { get; set; }

        /// <summary>
        /// 排序码
        /// </summary>
        public long? sortCode { get; set; }

        /// <summary>
        /// 状态(1-可用,0-禁用)
        /// </summary>
        public int? enabledMark { get; set; }

        /// <summary>
        /// 地图数据
        /// </summary>
        public string data { get; set; }
    }
}
