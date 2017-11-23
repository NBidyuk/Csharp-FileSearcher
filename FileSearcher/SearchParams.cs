using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace FileSearcher
{
    public class SearchParams
    {
        // -------Variables -----------


        private String fileName;
        private String searchPath;
        private Boolean subDirChecked;
        private String searchWord;


        // --------Constructor with parametres-----------

        public SearchParams (String userFileName, String userSearchPath, bool userSubDirChecked, String userSearchWord )
        {
            if (userFileName.Contains('*') || userFileName.Contains('.'))
                fileName = userFileName;
            else
                fileName = userFileName + '*';
          
            searchPath = userSearchPath;
            subDirChecked = userSubDirChecked;
            searchWord = userSearchWord;
        }


        // --------Properties-----------

        public String FileName
        {
            get { return fileName; }
        }


        public String SearchPath
        {
            get { return searchPath; }
        }


        public Boolean SubDirChecked
        {
            get { return subDirChecked; }

        }


        public String SearchWord
        {
            get { return searchWord; }
        }

    }
}
