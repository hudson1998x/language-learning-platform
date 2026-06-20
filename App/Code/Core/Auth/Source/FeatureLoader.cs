using LLE.Auth.Dto;
using LLE.Auth.Exceptions;
using LLE.Auth.Handlers;
using LLE.Kernel.Registry;

namespace LLE.Auth
{
    public static class FeatureLoader
    {
        public static void LoadFeatures()
        {
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
        }
    }
}