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
        }
    }
}