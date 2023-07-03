using JNPF.System.Entitys.Model.Permission.User;
using JNPF.System.Entitys.Permission;
using SqlSugar;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace JNPF.Common.Core.Manager
{
    /// <summary>
    /// 用户管理抽象
    /// </summary>
    public interface IUserManager
    {
        /// <summary>
        /// 用户编号
        /// </summary>
        string UserId { get; }

        /// <summary>
        /// 租户ID
        /// </summary>
        string TenantId { get; }


        /// <summary>
        /// 租户数据库名称
        /// </summary>
        string TenantDbName { get; }

        /// <summary>
        /// 用户账号
        /// </summary>
        string Account { get; }

        /// <summary>
        /// 用户昵称
        /// </summary>
        string RealName { get; }

        /// <summary>
        /// 当前用户 ToKen
        /// </summary>
        string ToKen { get; }

        /// <summary>
        /// 是否管理员
        /// </summary>
        bool IsAdministrator { get; }

        /// <summary>
        /// 用户信息
        /// </summary>
        UserEntity User { get; }

        /// <summary>
        /// 获取用户登录信息
        /// </summary>
        /// <returns></returns>
        Task<UserInfo> GetUserInfo();

        /// <summary>
        /// 获取数据条件(在线开发专用)
        /// </summary>
        /// <typeparam name="T">实体</typeparam>
        /// <param name="moduleId">模块ID</param>
        /// <returns></returns>
        List<IConditionalModel> GetCondition<T>(string primaryKey, string moduleId, bool isDataPermissions = true) where T : new();
    }
}
