using Riok.Mapperly.Abstractions;
using DemoApp.Web.Models;
using DemoApp.Web.ViewModels;

namespace DemoApp.Web.Mappings
{
    [Mapper]
    public partial class AppMapperImpl
    {
        [MapperIgnoreSource(nameof(UserFormViewModel.Roles))]
        [MapperIgnoreTarget(nameof(AppUser.Role))]
        [MapperIgnoreTarget(nameof(AppUser.InsUser))]
        [MapperIgnoreTarget(nameof(AppUser.InsDate))]
        [MapperIgnoreTarget(nameof(AppUser.UpdUser))]
        [MapperIgnoreTarget(nameof(AppUser.UpdDate))]
        public partial AppUser UserFormToEntity(UserFormViewModel source);

        [MapperIgnoreSource(nameof(AppUser.Role))]
        [MapperIgnoreSource(nameof(AppUser.RoleName))]
        [MapperIgnoreSource(nameof(AppUser.FullName))]
        [MapperIgnoreSource(nameof(AppUser.InsUser))]
        [MapperIgnoreSource(nameof(AppUser.InsDate))]
        [MapperIgnoreSource(nameof(AppUser.UpdUser))]
        [MapperIgnoreSource(nameof(AppUser.UpdDate))]
        [MapperIgnoreTarget(nameof(UserFormViewModel.Roles))]
        public partial UserFormViewModel EntityToUserForm(AppUser source);

        [MapperIgnoreSource(nameof(UserFormViewModel.Roles))]
        [MapperIgnoreTarget(nameof(AppUser.Role))]
        [MapperIgnoreTarget(nameof(AppUser.InsUser))]
        [MapperIgnoreTarget(nameof(AppUser.InsDate))]
        [MapperIgnoreTarget(nameof(AppUser.UpdUser))]
        [MapperIgnoreTarget(nameof(AppUser.UpdDate))]
        public partial void Map(UserFormViewModel source, [MappingTarget] AppUser target);

        [MapperIgnoreSource(nameof(PermissionFormViewModel.Roles))]
        [MapperIgnoreSource(nameof(PermissionFormViewModel.Menus))]
        [MapperIgnoreSource(nameof(PermissionFormViewModel.Operations))]
        [MapperIgnoreTarget(nameof(Permission.Role))]
        [MapperIgnoreTarget(nameof(Permission.Menu))]
        [MapperIgnoreTarget(nameof(Permission.InsUser))]
        [MapperIgnoreTarget(nameof(Permission.InsDate))]
        [MapperIgnoreTarget(nameof(Permission.UpdUser))]
        [MapperIgnoreTarget(nameof(Permission.UpdDate))]
        public partial Permission PermissionFormToEntity(PermissionFormViewModel source);

        [MapperIgnoreSource(nameof(PermissionFormViewModel.Roles))]
        [MapperIgnoreSource(nameof(PermissionFormViewModel.Menus))]
        [MapperIgnoreSource(nameof(PermissionFormViewModel.Operations))]
        [MapperIgnoreTarget(nameof(Permission.Role))]
        [MapperIgnoreTarget(nameof(Permission.Menu))]
        [MapperIgnoreTarget(nameof(Permission.InsUser))]
        [MapperIgnoreTarget(nameof(Permission.InsDate))]
        [MapperIgnoreTarget(nameof(Permission.UpdUser))]
        [MapperIgnoreTarget(nameof(Permission.UpdDate))]
        public partial void Map(PermissionFormViewModel source, [MappingTarget] Permission target);

        [MapperIgnoreSource(nameof(Permission.Role))]
        [MapperIgnoreSource(nameof(Permission.Menu))]
        [MapperIgnoreSource(nameof(Permission.InsUser))]
        [MapperIgnoreSource(nameof(Permission.InsDate))]
        [MapperIgnoreSource(nameof(Permission.UpdUser))]
        [MapperIgnoreSource(nameof(Permission.UpdDate))]
        [MapperIgnoreTarget(nameof(PermissionFormViewModel.Roles))]
        [MapperIgnoreTarget(nameof(PermissionFormViewModel.Menus))]
        [MapperIgnoreTarget(nameof(PermissionFormViewModel.Operations))]
        public partial PermissionFormViewModel EntityToPermissionForm(Permission source);
    }

    public class AppMapper
    {
        private readonly AppMapperImpl _impl = new();

        public AppUser UserFormToEntity(UserFormViewModel source) => _impl.UserFormToEntity(source);

        public UserFormViewModel EntityToUserForm(AppUser source) => _impl.EntityToUserForm(source);

        public void Map(UserFormViewModel source, AppUser target) => _impl.Map(source, target);

        public Permission PermissionFormToEntity(PermissionFormViewModel source) => _impl.PermissionFormToEntity(source);

        public void Map(PermissionFormViewModel source, Permission target) => _impl.Map(source, target);

        public PermissionFormViewModel EntityToPermissionForm(Permission source) => _impl.EntityToPermissionForm(source);
    }
}
