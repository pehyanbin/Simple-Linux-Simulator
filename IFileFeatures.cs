using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FileStorageSystem
{
    public interface IFileFeatures
    {
        void Rename(string newName);
        long GetSize();
        string GetFullPath();
    }
}