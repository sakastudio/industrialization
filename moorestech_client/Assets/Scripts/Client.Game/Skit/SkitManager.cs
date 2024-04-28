using System.Collections.Generic;
using Client.Game.InGame.Control;
using Client.Game.InGame.Player;
using Client.Skit;
using Client.Skit.Define;
using Client.Skit.SkitTrack;
using Client.Skit.UI;
using Cysharp.Threading.Tasks;
using UnityEngine;

namespace Client.Game.Skit
{
    public class SkitManager : MonoBehaviour
    {
        [SerializeField] private SkitUI skitUI;

        [SerializeField] private SkitCamera skitCamera;

        [SerializeField] private CharacterDefine characterDefine;
        [SerializeField] private VoiceDefine voiceDefine;

        public async UniTask StartStory(TextAsset storyCsv)
        {
            //前処理 Pre process
            var storyContext = PreProcess();
            var lines = storyCsv.text.Split('\n');
            Dictionary<string, int> tagIndexTable = CreateTagIndexTable(storyCsv.text.Split('\n'));

            //トラックの実行処理 Execute track
            for (var i = 0; i < lines.Length; i++)
            {
                var values = lines[i].Split(',');

                //トラックの取得と終了判定
                var trackKey = values[1];
                if (trackKey == string.Empty) continue; //空行はスキップ
                if (trackKey == "End") break;

                var track = StoryTrackDefine.GetStoryTrack(trackKey);
                if (track == null)
                {
                    Debug.LogError($"トラックが見つかりません : {trackKey}\nパラメータ : {string.Join(", ", values)}");
                    break;
                }

                //トラックの実行
                List<string> parameters = CreateParameter(values);
                var nextTag = await track.ExecuteTrack(storyContext, parameters);

                //タグがなかったのでそのまま継続
                if (nextTag == null) continue;

                //次のタグにジャンプ
                if (!tagIndexTable.TryGetValue(nextTag, out var nextIndex))
                {
                    Debug.LogError($"次のタグが見つかりません : トラック : {trackKey} 当該タグ : {nextTag}\nパラメータ : {string.Join(", ", values)}");
                    break;
                }
                i = nextIndex - 1;
            }

            //後処理 Post process
            skitUI.gameObject.SetActive(false);
            storyContext.DestroyCharacter();

            #region Internal

            StoryContext PreProcess()
            {
                //キャラクターを生成
                var characters = new Dictionary<string, SkitCharacter>();
                foreach (var characterInfo in characterDefine.CharacterInfos)
                {
                    var character = Instantiate(characterInfo.CharacterPrefab);
                    character.Initialize(transform, characterInfo.CharacterKey);
                    characters.Add(characterInfo.CharacterKey, character);
                }

                // 表示の設定
                skitUI.gameObject.SetActive(true);

                return new StoryContext(skitUI, characters, skitCamera, voiceDefine);
            }

            List<string> CreateParameter(string[] values)
            {
                var parameters = new List<string>();
                for (var j = 2; j < values.Length; j++)
                {
                    parameters.Add(values[j]);
                }

                return parameters;
            }

            Dictionary<string, int> CreateTagIndexTable(string[] lines)
            {
                var tagIndex = new Dictionary<string, int>();
                for (var i = 0; i < lines.Length; i++)
                {
                    var values = lines[i].Split(',');
                    var tag = values[0];
                    if (tag == string.Empty) continue;

                    if (tagIndex.ContainsKey(tag))
                    {
                        Debug.LogError($"タグが重複しています : {tag} {i}");
                        break;
                    }

                    tagIndex.Add(tag, i);
                }

                return tagIndex;
            }

            #endregion
        }
    }
}