using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Tomlyn;
using Tomlyn.Model;
using System.IO;
using System.Security.RightsManagement;

namespace LeetCodePlugin
{
    public class LeetcodeTomlUtil
    {
        public  string csrftoken;
        public  string LEETCODE_SESSION;
        public string last_edit_num;
        public string last_lang;

         TomlTable doc;

         string configFilePath;

        public static LeetcodeTomlUtil Instance { get; } = new LeetcodeTomlUtil();

        private LeetcodeTomlUtil()
        {
            string userDir = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);
            string dotleetcodeDir = Path.Combine(userDir, ".leetcode");
            configFilePath = Path.Combine(dotleetcodeDir, "leetcode.toml");
            var text = File.ReadAllText(configFilePath);
            doc = Toml.Parse(text).ToModel();
            var cookies = (TomlTable)doc["cookies"];
            // 读取值
            csrftoken = (string)cookies["csrf"];
            LEETCODE_SESSION = (string)cookies["session"];
            var code = (TomlTable)doc["code"];
            last_lang = (string)code["lang"];
            last_edit_num = (string)code["edit_num"];
        }

        ~LeetcodeTomlUtil()
        {
            saveAllValue();
        }

        public void saveAllValue()
        {
            var output = Toml.FromModel(doc);
            File.WriteAllText(configFilePath, output);
        }

        public  void modifyCsrftokenValue(string value)
        {
            var cookies = (TomlTable)doc["cookies"];
            cookies["csrf"] = value;
            csrftoken = (string)cookies["csrf"];
        }

        public  void modifyLEETCODE_SESSIONValue(string value)
        {
            var cookies = (TomlTable)doc["cookies"];
            cookies["session"] = value;
            csrftoken = (string)cookies["session"];
        }


        public  void modifyCodeLangValue(string value)
        {
            var cookies = (TomlTable)doc["code"];
            cookies["lang"] = value;
            csrftoken = (string)cookies["lang"];
        }

    }
}
