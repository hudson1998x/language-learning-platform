using LLE.Auth.Dto;
using LLE.Auth.Exceptions;
using LLE.Auth.Features.Roles;
using LLE.Auth.Features.Users;
using LLE.Auth.Handlers;
using LLE.Kernel.AutoEntity;
using LLE.Kernel.Registry;

namespace LLE.Auth
{
    public static class FeatureLoader
    {
        public static void LoadFeatures()
        {
            AutoEntityFeature.AutoFeature<User, IUserRepository>();
            AutoEntityFeature.AutoFeature<Role, IRoleRepository>();
            
            
            FeatureRegistry.Add(new Feature<LoginBody, LoginResponse>
            {
                FeatureName = "userLogin",
                FeatureGroup = "auth",
                Route = "/api/auth/login",
                Method = HttpMethod.Post,
                Handler = UserFeatures.Login,
                Catch = new Dictionary<Type, FeatureExceptionRule<LoginResponse>>()
                {
                    [typeof(LoginException)] = new()
                    {
                        Map = (exception) => new LoginResponse()
                        {
                            Success = false,
                            Message = exception.Message
                        }
                    }
                }
            });
            FeatureRegistry.Add(new Feature<object, UserAuthStateResponse>
            {
                FeatureName = "userState",
                FeatureGroup = "auth",
                Route = "/api/auth/state",
                Method = HttpMethod.Get,
                Handler = UserFeatures.UserAuthState,
                Catch = new Dictionary<Type, FeatureExceptionRule<UserAuthStateResponse>>()
                {
                    
                }
            });
            FeatureRegistry.Add(new Feature<RegisterBody, RegisterResponse>()
            {
                FeatureName = "register",
                FeatureGroup = "auth",
                Route = "/api/auth/register",
                Method = HttpMethod.Post,
                Handler = UserFeatures.Register,
                Catch = new Dictionary<Type, FeatureExceptionRule<RegisterResponse>>()
                {
                    [typeof(RegistrationException)] = new FeatureExceptionRule<RegisterResponse>()
                    {
                        Map = (exception) => new RegisterResponse()
                        {
                            Success = false,
                            Message = exception.Message
                        }
                    }
                }
            });
        }
    }
}