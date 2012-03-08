[assembly: WebActivator.PostApplicationStartMethod(typeof($rootnamespace$.App_Start.ElmahMvc3), "PostStart")]
namespace $rootnamespace$.App_Start
{    
    #region Imports
    using Elmah;
    #endregion
 
    public static class ElmahMvc3
    {
        public static void PostStart()
        {
            ElmahEnabledMvcApplication.Start();
        }
    }
}
