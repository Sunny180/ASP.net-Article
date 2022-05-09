namespace ArticleApi.Models
{
    public enum Status
    {
        success = 0,
        system_fail = -1,
        permission_denied = -2, // 沒有權限
        incorrect_account_or_password = -3,
        login_timeout = -4,
        data_not_found = -5,
        token_not_found = -6,
        token_err = -7,
        header_err = -8,
        invalid_token = -9,
        jwtuser_not_exist = -10,
        invalid_parameter = -11,
    }
}

