﻿using JNPF.Dependency;

namespace JNPF.System.Entitys.Dto.System.ModuleColumn
{
    /// <summary>
    /// 功能列修改输入
    /// </summary>
    [SuppressSniffer]
    public class ModuleColumnUpInput : ModuleColumnCrInput
    {
        /// <summary>
        /// id
        /// </summary>
        public string id { get; set; }
    }
}
