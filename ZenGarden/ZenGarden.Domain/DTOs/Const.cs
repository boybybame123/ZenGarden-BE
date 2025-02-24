using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ZenGarden.Domain.DTOs
{
    public class Const
    {
        public static string APIEndpoint = "https://localhost:7149/api/";
        public static string API_ENDPOINT = "https://localhost:7149/api/";
        #region Error Codes

        public static int ERROR_EXCEPTION = -4;

        #endregion

        #region Success Codes

        public static int SUCCESS_CREATE_CODE = 1;
        public static string SUCCESS_CREATE_MSG = "Save data success";
        public static int SUCCESS_READ_CODE = 1;
        public static string SUCCESS_READ_MSG = "Get data success";
        public static int SUCCESS_UPDATE_CODE = 1;
        public static string SUCCESS_UPDATE_MSG = "Update data success";
        public static int SUCCESS_DELETE_CODE = 1;
        public static string SUCCESS_DELETE_MSG = "Delete data success";


        #endregion

        #region Fail code

        public static int FAIL_CREATE_CODE = -1;
        public static string FAIL_CREATE_MSG = "Save data fail";
        public static int FAIL_READ_CODE = -1;
        public static string FAIL_READ_MSG = "Get data fail";
        public static int FAIL_UPDATE_CODE = -1;
        public static string FAIL_UPDATE_MSG = "Update data fail";
        public static int FAIL_DELETE_CODE = -1;
        public static string FAIL_DELETE_MSG = "Delete data fail";

        #endregion

        #region Warning Code

        public static int WARNING_NO_DATA_CODE = 4;
        public static string WARNING_NO_DATA_MSG = "No data";

        #endregion

        #region Entity Id

        public static string CONSIGNMENT = "CONSIGN";
        public static string CONSIGNMENT_INDEX = "ConsignmentId";
        public static string IMAGE = "IMG";
        public static string IMAGE_INDEX = "ImageId";
        public static string KOIFISH = "KOI";
        public static string KOIFISH_INDEX = "KoiId";
        public static string ORDER = "ORDER";
        public static string ORDER_INDEX = "OrderId";
        public static string PAYMENT = "TRANS";
        public static string PAYMENT_INDEX = "PaymentId";
        public static string USER = "USER";
        public static string USER_INDEX = "UserId";
        public static string VOUCHER = "VOUCH";
        public static string VOUCHER_INDEX = "VoucherId";

        #endregion
    }
}
