﻿using JNPF.Common.Const;
using JNPF.Common.Entity;
using SqlSugar;

namespace JNPF.System.Entitys.System
{
    /// <summary>
    /// 字典数据
    /// 版 本：V3.2
    /// 版 权：引迈信息技术有限公司（https://www.jnpfsoft.com）
    /// 作 者：JNPF开发平台组
    /// 日 期：2021-06-01 
    /// </summary>
    [SugarTable("BASE_DICTIONARYDATA")]
    [Tenant(ClaimConst.TENANT_ID)]
    public class DictionaryDataEntity : CLDEntityBase
    {
        /// <summary>
        /// 上级
        /// </summary>
        [SugarColumn(ColumnName = "F_PARENTID")]
        public string ParentId { get; set; }

        /// <summary>
        /// 名称
        /// </summary>
        [SugarColumn(ColumnName = "F_FULLNAME")]
        public string FullName { get; set; }

        /// <summary>
        /// 编码
        /// </summary>
        [SugarColumn(ColumnName = "F_ENCODE")]
        public string EnCode { get; set; }

        /// <summary>
        /// 拼音
        /// </summary>
        [SugarColumn(ColumnName = "F_SIMPLESPELLING")]
        public string SimpleSpelling { get; set; }

        /// <summary>
        /// 默认
        /// </summary>
        [SugarColumn(ColumnName = "F_ISDEFAULT")]
        public int? IsDefault { get; set; }

        /// <summary>
        /// 描述
        /// </summary>
        [SugarColumn(ColumnName = "F_DESCRIPTION")]
        public string Description { get; set; }

        /// <summary>
        /// 排序码
        /// </summary>
        [SugarColumn(ColumnName = "F_SORTCODE")]
        public long? SortCode { get; set; }

        /// <summary>
        /// 类别主键
        /// </summary>
        [SugarColumn(ColumnName = "F_DICTIONARYTYPEID")]
        public string DictionaryTypeId { get; set; }
    }
}
