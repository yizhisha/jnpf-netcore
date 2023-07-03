using JNPF.Common.Const;
using JNPF.Common.Enum;
using JNPF.Common.Helper;
using JNPF.Dependency;
using JNPF.System.Entitys.Enum;
using JNPF.System.Entitys.Model.Permission.Authorize;
using JNPF.System.Entitys.Model.Permission.User;
using JNPF.System.Entitys.Permission;
using JNPF.System.Entitys.System;
using JNPF.System.Interfaces.Permission;
using JNPF.System.Interfaces.System;
using Microsoft.AspNetCore.Http;
using SqlSugar;
using System.Collections.Generic;
using System.Threading.Tasks;
using Enums = System.Enum;

namespace JNPF.Common.Core.Manager
{
    /// <summary>
    /// 用户管理
    /// </summary>
    public class UserManager : IUserManager, IScoped
    {
        private readonly IUsersService _userService; // 用户服务

        private readonly ISysCacheService _sysCacheService;

        private readonly ISqlSugarRepository<AuthorizeEntity> _authorizeRepository; //权限操作表仓储
        private readonly ISqlSugarRepository<ModuleDataAuthorizeSchemeEntity> _moduleDataAuthorizeSchemeRepository;

        private readonly HttpContext _httpContext;

        /// <summary>
        /// 用户信息
        /// </summary>
        public UserEntity User
        {
            get => _userService.GetInfoByUserId(UserId);
        }

        /// <summary>
        /// 用户ID
        /// </summary>
        public string UserId
        {
            get => _httpContext.User.FindFirst(ClaimConst.CLAINM_USERID)?.Value;
        }

        /// <summary>
        /// 用户账号
        /// </summary>
        public string Account
        {
            get => _httpContext.User.FindFirst(ClaimConst.CLAINM_ACCOUNT)?.Value;
        }

        /// <summary>
        /// 用户昵称
        /// </summary>
        public string RealName
        {
            get => _httpContext.User.FindFirst(ClaimConst.CLAINM_REALNAME)?.Value;
        }

        /// <summary>
        /// 当前用户 token
        /// </summary>
        public string ToKen
        {
            get => _httpContext.Request.Headers["Authorization"];
        }

        public string TenantId
        {
            get => _httpContext.User.FindFirst(ClaimConst.TENANT_ID)?.Value;
        }

        /// <summary>
        /// 是否是管理员
        /// </summary>
        public bool IsAdministrator
        {
            get => _httpContext.User.FindFirst(ClaimConst.CLAINM_ADMINISTRATOR)?.Value == ((int)AccountType.Administrator).ToString();
        }

        /// <summary>
        /// 租户数据库名称
        /// </summary>
        public string TenantDbName
        {
            get => _httpContext.User.FindFirst(ClaimConst.TENANT_DB_NAME)?.Value;
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="userService"></param>
        /// <param name="sysCacheService"></param>
        /// <param name="httpContextAccessor"></param>
        public UserManager(IUsersService userService, ISysCacheService sysCacheService, ISqlSugarRepository<AuthorizeEntity> authorizeRepository,
                        ISqlSugarRepository<ModuleDataAuthorizeSchemeEntity> moduleDataAuthorizeSchemeRepository)
        {
            _userService = userService;
            _sysCacheService = sysCacheService;
            _authorizeRepository = authorizeRepository;
            _moduleDataAuthorizeSchemeRepository = moduleDataAuthorizeSchemeRepository;
            _httpContext = App.HttpContext;
        }

        /// <summary>
        /// 获取用户登录信息
        /// </summary>
        /// <returns></returns>
        public async Task<UserInfo> GetUserInfo()
        {
            var data = new UserInfo();
            var userCache = await _sysCacheService.GetUserInfo(TenantId + "_" + UserId);
            if (userCache == null)
            {
                data = await _userService.GetUserInfo(UserId, TenantId);
            }
            else
            {
                data = userCache;
            }
            return data;
        }

        /// <summary>
        /// 获取数据条件(在线开发专用)
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="moduleId"></param>
        /// <returns></returns>
        public List<IConditionalModel> GetCondition<T>(string primaryKey, string moduleId, bool isDataPermissions = true) where T : new()
        {
            var userInfo = GetUserInfo().Result;
            var conModels = new List<IConditionalModel>();
            if (this.IsAdministrator)
                return conModels;

            var items = _authorizeRepository.Context.Queryable<AuthorizeEntity, RoleEntity>((a, b) => new JoinQueryInfos(JoinType.Left, b.Id == a.ObjectId && b.EnabledMark == 1 && b.DeleteMark == null))
                       .In((a, b) => b.Id, userInfo.roleIds)
                       .Where(a => a.ItemType == "resource")
                       .GroupBy(a => new { a.ItemId }).Select(a => a.ItemId).ToList();

            if (isDataPermissions == false)
            {
                conModels.Add(new ConditionalCollections()
                {
                    ConditionalList = new List<KeyValuePair<WhereType, SqlSugar.ConditionalModel>>()
                    {
                        new KeyValuePair<WhereType, ConditionalModel>(WhereType.And, new ConditionalModel() { FieldName = primaryKey, ConditionalType = ConditionalType.NoEqual, FieldValue = "0", FieldValueConvertFunc = it => SqlSugar.UtilMethods.ChangeType2(it, typeof(string)) })
                    }
                });
                return conModels;
            }
            else if (items.Count == 0 && isDataPermissions == true)
            {
                conModels.Add(new ConditionalCollections()
                {
                    ConditionalList = new List<KeyValuePair<WhereType, SqlSugar.ConditionalModel>>()
                    {
                        new KeyValuePair<WhereType, ConditionalModel>(WhereType.And, new ConditionalModel() { FieldName = primaryKey, ConditionalType = ConditionalType.Equal, FieldValue = "0", FieldValueConvertFunc = it => SqlSugar.UtilMethods.ChangeType2(it, typeof(string)) })
                    }
                });
                return conModels;
            }

            var resourceList = _moduleDataAuthorizeSchemeRepository.AsQueryable().In(it => it.Id, items).Where(it => it.ModuleId == moduleId && it.DeleteMark == null).ToList();

            //方案和方案，分组和分组 之间 必须要 用 And 条件 拼接
            var isAnd = false;

            foreach (var item in resourceList)
            {
                var conditionModelList = item.ConditionJson.ToList<AuthorizeModuleResourceConditionModel>();
                foreach (var conditionItem in conditionModelList)
                {
                    foreach (var fieldItem in conditionItem.Groups)
                    {
                        var itemField = fieldItem.Field;
                        var itemValue = fieldItem.Value;
                        var itemMethod = (SearchMethod)Enums.Parse(typeof(SearchMethod), fieldItem.Op);

                        switch (itemValue.ToString())
                        {
                            //当前用户
                            case "@userId":
                                {
                                    switch (conditionItem.Logic)
                                    {
                                        case "and":
                                            conModels.Add(new ConditionalCollections()
                                            {
                                                ConditionalList = new List<KeyValuePair<WhereType, SqlSugar.ConditionalModel>>() {
                                                    new KeyValuePair<WhereType, ConditionalModel>(WhereType.And, GetConditionalModel(itemMethod, itemField, userInfo.userId))
                                                }
                                            });
                                            break;
                                        case "or":
                                            conModels.Add(new ConditionalCollections()
                                            {
                                                ConditionalList = new List<KeyValuePair<WhereType, SqlSugar.ConditionalModel>>() {
                                                    new KeyValuePair<WhereType, ConditionalModel>(isAnd ? WhereType.And : WhereType.Or, GetConditionalModel(itemMethod, itemField, userInfo.userId))
                                                }
                                            });
                                            break;
                                    }
                                }
                                break;
                            //当前用户集下属
                            case "@userAraSubordinates":
                                {
                                    switch (conditionItem.Logic)
                                    {
                                        case "and":
                                            conModels.Add(new ConditionalCollections()
                                            {
                                                ConditionalList = new List<KeyValuePair<WhereType, SqlSugar.ConditionalModel>>() {
                                                   new KeyValuePair<WhereType, ConditionalModel>(WhereType.And, GetConditionalModel(itemMethod, itemField, userInfo.userId)),
                                                   new KeyValuePair<WhereType, ConditionalModel>(WhereType.Or, GetConditionalModel(SearchMethod.In, itemField, string.Join(",", userInfo.subordinates)))
                                                }
                                            });
                                            break;
                                        case "or":
                                            conModels.Add(new ConditionalCollections()
                                            {
                                                ConditionalList = new List<KeyValuePair<WhereType, SqlSugar.ConditionalModel>>() {
                                                   new KeyValuePair<WhereType, ConditionalModel>(isAnd ? WhereType.And : WhereType.Or, GetConditionalModel(itemMethod, itemField, userInfo.userId)),
                                                   new KeyValuePair<WhereType, ConditionalModel>(WhereType.Or, GetConditionalModel(SearchMethod.In, itemField, string.Join(",", userInfo.subordinates)))
                                                }
                                            });
                                            break;
                                    }
                                }
                                break;
                            //当前组织
                            case "@organizeId":
                                {
                                    if (!string.IsNullOrEmpty(userInfo.organizeId))
                                        switch (conditionItem.Logic)
                                        {
                                            case "and":
                                                conModels.Add(new ConditionalCollections()
                                                {
                                                    ConditionalList = new List<KeyValuePair<WhereType, SqlSugar.ConditionalModel>>()
                                                    {
                                                        new KeyValuePair<WhereType, ConditionalModel>(WhereType.And, GetConditionalModel(itemMethod, itemField, userInfo.organizeId))
                                                    }
                                                });
                                                break;
                                            case "or":
                                                conModels.Add(new ConditionalCollections()
                                                {
                                                    ConditionalList = new List<KeyValuePair<WhereType, SqlSugar.ConditionalModel>>()
                                                    {
                                                        new KeyValuePair<WhereType, ConditionalModel>(isAnd ? WhereType.And : WhereType.Or, GetConditionalModel(itemMethod, itemField, userInfo.organizeId))
                                                    }
                                                });
                                                break;
                                        }
                                }
                                break;
                            //当前组织及子组织
                            case "@organizationAndSuborganization":
                                {
                                    if (!string.IsNullOrEmpty(userInfo.organizeId))
                                        switch (conditionItem.Logic)
                                        {
                                            case "and":
                                                conModels.Add(new ConditionalCollections()
                                                {
                                                    ConditionalList = new List<KeyValuePair<WhereType, SqlSugar.ConditionalModel>>()
                                                    {
                                                        new KeyValuePair<WhereType, ConditionalModel>(WhereType.And, GetConditionalModel(itemMethod, itemField, userInfo.organizeId)),
                                                        new KeyValuePair<WhereType, ConditionalModel>(WhereType.Or, GetConditionalModel(SearchMethod.In, itemField, string.Join(",", userInfo.subsidiary)))
                                                    }
                                                });
                                                break;
                                            case "or":
                                                conModels.Add(new ConditionalCollections()
                                                {
                                                    ConditionalList = new List<KeyValuePair<WhereType, SqlSugar.ConditionalModel>>()
                                                    {
                                                        new KeyValuePair<WhereType, ConditionalModel>(isAnd ? WhereType.And : WhereType.Or, GetConditionalModel(itemMethod, itemField, userInfo.organizeId)),
                                                        new KeyValuePair<WhereType, ConditionalModel>(WhereType.Or, GetConditionalModel(SearchMethod.In, itemField, string.Join(",", userInfo.subsidiary)))
                                                    }
                                                });
                                                break;
                                        }
                                }
                                break;
                            default:
                                {
                                    if (!string.IsNullOrEmpty(itemValue))
                                        switch (conditionItem.Logic)
                                        {
                                            case "and":
                                                conModels.Add(new ConditionalCollections()
                                                {
                                                    ConditionalList = new List<KeyValuePair<WhereType, SqlSugar.ConditionalModel>>()
                                                    {
                                                        new KeyValuePair<WhereType, ConditionalModel>(WhereType.And, GetConditionalModel(itemMethod, itemField, itemValue, fieldItem.Type))
                                                    }
                                                });
                                                break;
                                            case "or":
                                                conModels.Add(new ConditionalCollections()
                                                {
                                                    ConditionalList = new List<KeyValuePair<WhereType, SqlSugar.ConditionalModel>>()
                                                    {
                                                        new KeyValuePair<WhereType, ConditionalModel>(isAnd ? WhereType.And : WhereType.Or, GetConditionalModel(itemMethod, itemField, itemValue, fieldItem.Type))
                                                    }
                                                });
                                                break;
                                        }
                                }
                                break;
                        }
                        isAnd = false;
                    }
                    isAnd = true;//分组和分组
                }
                isAnd = true;//方案和方案
            }
            if (resourceList.Count == 0)
            {
                conModels.Add(new ConditionalCollections()
                {
                    ConditionalList = new List<KeyValuePair<WhereType, SqlSugar.ConditionalModel>>()
                    {
                        new KeyValuePair<WhereType, ConditionalModel>(WhereType.And, new ConditionalModel() { FieldName = primaryKey, ConditionalType = ConditionalType.Equal, FieldValue = "0", FieldValueConvertFunc = it => SqlSugar.UtilMethods.ChangeType2(it, typeof(string)) })
                    }
                });
            }
            return conModels;
        }

        /// <summary>
        ///
        /// </summary>
        /// <returns></returns>
        private ConditionalModel GetConditionalModel(SearchMethod expressType, string fieldName, string fieldValue, string dataType = "string")
        {
            switch (expressType)
            {
                //like
                case SearchMethod.Contains:
                    return new ConditionalModel() { FieldName = fieldName, ConditionalType = ConditionalType.Like, FieldValue = fieldValue };
                //等于
                case SearchMethod.Equal:
                    switch (dataType)
                    {
                        case "Double":
                            return new ConditionalModel() { FieldName = fieldName, ConditionalType = ConditionalType.Equal, FieldValue = fieldValue, FieldValueConvertFunc = it => SqlSugar.UtilMethods.ChangeType2(it, typeof(double)) };
                        case "Int32":
                            return new ConditionalModel() { FieldName = fieldName, ConditionalType = ConditionalType.Equal, FieldValue = fieldValue, FieldValueConvertFunc = it => SqlSugar.UtilMethods.ChangeType2(it, typeof(int)) };
                        default:
                            return new ConditionalModel() { FieldName = fieldName, ConditionalType = ConditionalType.Equal, FieldValue = fieldValue };
                    }
                //不等于
                case SearchMethod.NotEqual:
                    switch (dataType)
                    {
                        case "Double":
                            return new ConditionalModel() { FieldName = fieldName, ConditionalType = ConditionalType.NoEqual, FieldValue = fieldValue, FieldValueConvertFunc = it => SqlSugar.UtilMethods.ChangeType2(it, typeof(double)) };
                        case "Int32":
                            return new ConditionalModel() { FieldName = fieldName, ConditionalType = ConditionalType.NoEqual, FieldValue = fieldValue, FieldValueConvertFunc = it => SqlSugar.UtilMethods.ChangeType2(it, typeof(int)) };
                        default:
                            return new ConditionalModel() { FieldName = fieldName, ConditionalType = ConditionalType.NoEqual, FieldValue = fieldValue };
                    }
                //小于
                case SearchMethod.LessThan:
                    switch (dataType)
                    {
                        case "Double":
                            return new ConditionalModel() { FieldName = fieldName, ConditionalType = ConditionalType.LessThan, FieldValue = fieldValue, FieldValueConvertFunc = it => SqlSugar.UtilMethods.ChangeType2(it, typeof(double)) };
                        case "Int32":
                            return new ConditionalModel() { FieldName = fieldName, ConditionalType = ConditionalType.LessThan, FieldValue = fieldValue, FieldValueConvertFunc = it => SqlSugar.UtilMethods.ChangeType2(it, typeof(int)) };
                        default:
                            return new ConditionalModel() { FieldName = fieldName, ConditionalType = ConditionalType.LessThan, FieldValue = fieldValue };
                    }
                //小于等于
                case SearchMethod.LessThanOrEqual:
                    switch (dataType)
                    {
                        case "Double":
                            return new ConditionalModel() { FieldName = fieldName, ConditionalType = ConditionalType.LessThanOrEqual, FieldValue = fieldValue, FieldValueConvertFunc = it => SqlSugar.UtilMethods.ChangeType2(it, typeof(double)) };
                        case "Int32":
                            return new ConditionalModel() { FieldName = fieldName, ConditionalType = ConditionalType.LessThanOrEqual, FieldValue = fieldValue, FieldValueConvertFunc = it => SqlSugar.UtilMethods.ChangeType2(it, typeof(int)) };
                        default:
                            return new ConditionalModel() { FieldName = fieldName, ConditionalType = ConditionalType.LessThanOrEqual, FieldValue = fieldValue };
                    }
                //大于
                case SearchMethod.GreaterThan:
                    switch (dataType)
                    {
                        case "Double":
                            return new ConditionalModel() { FieldName = fieldName, ConditionalType = ConditionalType.GreaterThan, FieldValue = fieldValue, FieldValueConvertFunc = it => SqlSugar.UtilMethods.ChangeType2(it, typeof(double)) };
                        case "Int32":
                            return new ConditionalModel() { FieldName = fieldName, ConditionalType = ConditionalType.GreaterThan, FieldValue = fieldValue, FieldValueConvertFunc = it => SqlSugar.UtilMethods.ChangeType2(it, typeof(int)) };
                        default:
                            return new ConditionalModel() { FieldName = fieldName, ConditionalType = ConditionalType.GreaterThan, FieldValue = fieldValue };
                    }
                //大于等于
                case SearchMethod.GreaterThanOrEqual:
                    switch (dataType)
                    {
                        case "Double":
                            return new ConditionalModel() { FieldName = fieldName, ConditionalType = ConditionalType.GreaterThanOrEqual, FieldValue = fieldValue, FieldValueConvertFunc = it => SqlSugar.UtilMethods.ChangeType2(it, typeof(double)) };
                        case "Int32":
                            return new ConditionalModel() { FieldName = fieldName, ConditionalType = ConditionalType.GreaterThanOrEqual, FieldValue = fieldValue, FieldValueConvertFunc = it => SqlSugar.UtilMethods.ChangeType2(it, typeof(int)) };
                        default:
                            return new ConditionalModel() { FieldName = fieldName, ConditionalType = ConditionalType.GreaterThanOrEqual, FieldValue = fieldValue };
                    }
                //包含
                case SearchMethod.In:
                    return new ConditionalModel() { FieldName = fieldName, ConditionalType = ConditionalType.In, FieldValue = fieldValue };
            }
            return new ConditionalModel();
        }

    }
}
