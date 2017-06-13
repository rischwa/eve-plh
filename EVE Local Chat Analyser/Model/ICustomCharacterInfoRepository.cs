using System.Collections.Generic;
using EveLocalChatAnalyser.Utilities;

namespace EveLocalChatAnalyser.Model
{
    public interface ICustomCharacterInfoRepository
    {
        CustomCharacterInfoViewModel GetCustomCharacterInfo(IEveCharacter character);

        void UpsertCustomCharacterInfo(CustomCharacterInfo info);
    }

    public class CustomCharacterInfoRepository : ICustomCharacterInfoRepository
    {
        private readonly IDictionary<long, CustomCharacterInfoViewModel> _viewModelsCache = new Dictionary<long, CustomCharacterInfoViewModel>();
        public CustomCharacterInfoViewModel GetCustomCharacterInfo(IEveCharacter character)
        {
            var id = long.Parse(character.Id);
            CustomCharacterInfoViewModel result;
            if (_viewModelsCache.TryGetValue(id, out result))
            {
                return result;
            }
            var customCharacterInfo = App.GetFromCollection<CustomCharacterInfo, CustomCharacterInfo>(x => x.FindById(long.Parse(character.Id)))
                                                      ?? new CustomCharacterInfo
                                                         {Id = id};
            result = new CustomCharacterInfoViewModel(customCharacterInfo);
            _viewModelsCache[id] = result;

            return result;
        }

        public void UpsertCustomCharacterInfo(CustomCharacterInfo info)
        {
            //TODO create exec in collection or something
            App.GetFromCollection<CustomCharacterInfo, bool>(
                                                             x =>
                                                             {
                                                                 x.Upsert(info);
                                                                 return true;
                                                             });
        }
    }
}
