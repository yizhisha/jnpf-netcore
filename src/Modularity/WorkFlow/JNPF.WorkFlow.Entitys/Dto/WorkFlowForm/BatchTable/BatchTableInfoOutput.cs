﻿using JNPF.Dependency;
using System;

namespace JNPF.WorkFlow.Entitys.Dto.WorkFlowForm.BatchTable
{
    [SuppressSniffer]
    public class BatchTableInfoOutput
    {
        /// <summary>
        /// 
        /// </summary>
        public string billNo { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public string description { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public string draftedPerson { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public string fileJson { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public string fileTitle { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public string fillNum { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public string flowId { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public string flowTitle { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public int? flowUrgent { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public string id { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public string sendUnit { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public string shareNum { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public string typing { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public DateTime writingDate { get; set; }
    }
}
