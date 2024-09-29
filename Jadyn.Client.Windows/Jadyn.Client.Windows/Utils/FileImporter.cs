using Jadyn.Common.Models;
using System.Collections;
using System.Collections.Generic;
using Windows.Storage;

namespace Jadyn.Client.Windows.Utils
{
    public interface IFileImporter
    {
        IEnumerable<TModel> ImportModelsFromFile<TModel>(StorageFile file) where TModel : BaseModel;
    }

    public class FileImporter : IFileImporter
    {
        public IEnumerable<TModel> ImportModelsFromFile<TModel>(StorageFile file) where TModel : BaseModel
        {
            var result = new List<TModel>();
            return result;
        }
    }
}
