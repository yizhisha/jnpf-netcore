﻿namespace JNPF.VisualDev.Entitys.Dto.Portal
{
    /// <summary>
    /// 门户设计信息输出
    /// </summary>
    public class PortalInfoOutput
    {
        /// <summary>
        /// ID
        /// </summary>
        public string id { get; set; }

        /// <summary>
        /// 名称
        /// </summary>
        public string fullName { get; set; }

        /// <summary>
        /// 分类
        /// </summary>
        public string category { get; set; }

        /// <summary>
        /// 编码
        /// </summary>
        public string enCode { get; set; }

        /// <summary>
        /// 是否可用
        /// 0-不可用，1-可用
        /// </summary>
        public int enabledMark { get; set; } = 0;

        /// <summary>
        /// 说明
        /// </summary>
        public string description { get; set; }

        /// <summary>
        /// 表单JSON
        /// </summary>
        public string formData { get; set; }

        /// <summary>
        /// 排序
        /// </summary>
        public long sortCode { get; set; }
    }
}
