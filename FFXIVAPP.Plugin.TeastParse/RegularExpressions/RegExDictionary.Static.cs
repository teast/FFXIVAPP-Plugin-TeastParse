/*
   This file contains shared list regarding parsing of chat strings that can take place.
   It also contains some shared properties regarding filtering (for example on subject or monster)
*/
using System;

namespace FFXIVAPP.Plugin.TeastParse.RegularExpressions
{
    internal partial class RegExDictionary
    {
        /// <summary>
        /// All <see cref="ChatCodeSubject" /> that is interesting regarding an "player"
        /// </summary>
        public readonly static ChatCodeSubject SubjectPlayer =
            ChatCodeSubject.Alliance | ChatCodeSubject.Party | ChatCodeSubject.You |
            ChatCodeSubject.Pet | ChatCodeSubject.PetAlliance | ChatCodeSubject.PetParty |
            ChatCodeSubject.PetOther | ChatCodeSubject.Other | ChatCodeSubject.NPC |
            ChatCodeSubject.Other | ChatCodeSubject.Unknown;

        /// <summary>
        /// All <see cref="ChatCodeSubject" /> that is interesting regarding an "monster"
        /// </summary>
        public readonly static ChatCodeSubject SubjectMonster =
            ChatCodeSubject.Engaged | ChatCodeSubject.UnEngaged | ChatCodeSubject.Unknown;

        /// <summary>
        /// Regex for parsing an action string
        /// </summary>
        /// <returns>
        /// Regex Match Groups:
        /// source, name of the one who did the action
        /// action, name of the action
        /// </returns>
        public readonly static RegExTypePair ActionsPlayer = new RegExTypePair(SubjectPlayer, null,
            Tuple.Create(GameLanguageEnum.German, @"^(?<source>Du|.+) (setzt (?<action>.+) ein|wirks?t (?<action>.+))\.$"),
            Tuple.Create(GameLanguageEnum.English, @"^(?<source>You|.+) (use|cast)s? (?<action>.+)\.$"),
            Tuple.Create(GameLanguageEnum.France, @"^(?<source>Vous|.+) (utilise|lance)z? (?<action>.+)\.$"),
            Tuple.Create(GameLanguageEnum.Japanese, @"^(?<source>.+)の「(?<action>.+)」$"),
            Tuple.Create(GameLanguageEnum.Chinese, @"^:(?<source>You|.+)(发动了|咏唱了|正在咏唱|正在发动)“(?<action>.+)”。$"));

        /// <summary>
        /// Regex for parsing an action string
        /// </summary>
        /// <returns>
        /// Regex Match Groups:
        /// source, name of the one who did the action
        /// action, name of the action
        /// </returns>
        public readonly static RegExTypePair ActionsMonster = new RegExTypePair(SubjectMonster, null,
            Tuple.Create(GameLanguageEnum.German, @"^(D(u|einer|(i|e)r|ich|as|ie|en) )?(?<source>.+) (setzt (?<action>.+) ein|wirks?t (?<action>.+))\.$"),
            Tuple.Create(GameLanguageEnum.English, @"^((T|t)he )?(?<source>.+) (use|cast)s? (?<action>.+)\.$"),
            Tuple.Create(GameLanguageEnum.France, @"^(L[aes] |[LEAD]')?(?<source>.+) (utilise|lance)z? (?<action>.+)\.$"),
            Tuple.Create(GameLanguageEnum.Japanese, @"^(?<source>.+)の「(?<action>.+)」$"),
            Tuple.Create(GameLanguageEnum.Chinese, @"^:(?<source>.+)(发动了|咏唱了|正在咏唱|正在发动)“(?<action>.+)”。$"));

        /// <summary>
        /// Regex for parsing an damage done from action
        /// </summary>
        /// <returns>
        /// Regex Match Groups:
        /// block - contains text if there is a block
        /// parry - contains text if there is an parry
        /// crit - contains text if there is an critical hit
        /// direct - contains text if the hit was an direct hit
        /// target - name of one receiving the damage
        /// amount - amount of damage
        /// modifier - how many percent modifier
        /// </returns>
        public readonly static RegExTypePair DamagePlayerAction = new RegExTypePair(SubjectPlayer, null,
            Tuple.Create(GameLanguageEnum.German, @"^ ⇒ (?<block>Geblockt! ?)?(?<parry>Pariert! ?)?(?<crit>Kritischer Treffer! ?)?(D(u|einer|(i|e)r|ich|as|ie|en) )?(?<target>.+) erleides?t (nur )?(?<amount>\d+) ?(\((?<modifier>.\d+)%\) )?Punkte? (Schaden|reduziert)\.$"),
            Tuple.Create(GameLanguageEnum.English, @"^ ⇒ (?<block>Blocked! )?(?<parry>Parried! )?(?<crit>Critical(?<direct> direct hit)?! )?(?<direct>Direct hit! )?((T|t)he )?(?<target>.+) takes? (?<amount>\d+) ?(\((?<modifier>.\d+)%\) )?damage\.$"),
            Tuple.Create(GameLanguageEnum.France, @"^ ⇒ (?<parry>Parade ?! )?(?<block>Blocage ?! )?(?<crit>Critique ?! )?(L[aes] |[LEAD]')?(?<target>.+) subit (?<amount>\d+) ?(\((?<modifier>.\d+)%\) )?points? de dégâts?\.$"),
            Tuple.Create(GameLanguageEnum.Japanese, @"^ ⇒ (?<crit>クリティカル！ )?(?<target>.+)((に|は)、?)(?<block>ブロックした！ )?(?<parry>受け流した！ )?(?<amount>\d+) ?(\((?<modifier>.\d+)%\) ?)?ダメージ。$"),
            Tuple.Create(GameLanguageEnum.Chinese, @"^: ⇒ (?<crit>暴击！ )?(?<target>.+?)(?<block>招架住了！ )?(?<parry>格挡住了！ )?(受到(了)?)(?<amount>\d+) ?(\((?<modifier>.\d+)%\) ?)?点伤害。$"));

        /// <summary>
        /// Regex for parsing an auto-attack damage
        /// </summary>
        /// <returns>
        /// Regex Match Groups:
        /// block - contains text if there is a block
        /// parry - contains text if there is an parry
        /// crit - contains text if there is an critical hit
        /// direct - contains text if the hit was an direct hit
        /// source - name of the one doing the damage
        /// target - name of one receiving the damage
        /// amount - amount of damage
        /// modifier - how many percent modifier
        /// </returns>
        public readonly static RegExTypePair DamagePlayerAutoAttack = new RegExTypePair(SubjectPlayer, null,
            Tuple.Create(GameLanguageEnum.German, @"^(?! ⇒)(?<block>Geblockt! ?)?(?<parry>Pariert! ?)?(?<crit>Kritischer Treffer! ?)?(?<source>Du|.+) triffs?t (d(u|einer|(i|e)r|ich|as|ie|en) )?(?<target>.+) und verursachs?t (?<amount>\d+) ?(\((?<modifier>.\d+)%\) )?Punkte? (Schaden|reduziert)\.$"),
            Tuple.Create(GameLanguageEnum.English, @"^(?! ⇒)(?<block>Blocked! )?(?<parry>Parried! )?(?<crit>Critical(?<direct> direct hit)?! )?(?<direct>Direct hit! )?(?<source>You|.+) hits? ((T|t)he )?(?<target>.+) for (?<amount>\d+) ?(\((?<modifier>.\d+)%\) )?damage\.$"),
            Tuple.Create(GameLanguageEnum.France, @"^(?! ⇒)(?<parry>Parade ?! )?(?<block>Blocage ?! )?(?<crit>Critique ?! )?(?<source>Vous|.+) infligez? \w+ (l[aes] |[lead]')?(?<target>.+) (?<amount>\d+) ?(\((?<modifier>.\d+)%\) )?points? de dégâts?\.$"),
            Tuple.Create(GameLanguageEnum.Japanese, @"^(?<source>.+)の攻撃( ⇒ )?(?<crit>クリティカル！ )?(?<target>.+)((に|は)、?)(?<block>ブロックした！ )?(?<parry>受け流した！ )?(?<amount>\d+) ?(\((?<modifier>.\d+)%\) ?)?ダメージ。$"),
            Tuple.Create(GameLanguageEnum.Chinese, @"^:(?<source>.+)发动攻击( ⇒ )?(?<crit>暴击！ )?(?<target>.+?)(?<block>招架住了！ )?(?<parry>格挡住了！ )?(受到(了)?)(?<amount>\d+) ?(\((?<modifier>.\d+)%\) ?)?点伤害。$"));

        /// <summary>
        /// Regex for parsing an damage done from action
        /// </summary>
        /// <returns>
        /// Regex Match Groups:
        /// block - contains text if there is a block
        /// parry - contains text if there is an parry
        /// crit - contains text if there is an critical hit
        /// target - name of one receiving the damage
        /// amount - amount of damage
        /// modifier - how many percent modifier
        /// </returns>
        public readonly static RegExTypePair DamageMonsterAction = new RegExTypePair(SubjectMonster, null,
            Tuple.Create(GameLanguageEnum.German, @"^ ⇒ (?<block>Geblockt! ?)?(?<parry>Pariert! ?)?(?<crit>Kritischer Treffer! ?)?(?<target>dich|.+)( erleides?t (nur )?|, aber der Schaden wird auf )(?<amount>\d+) ?(\((?<modifier>.\d+)%\) )?Punkte? (Schaden|reduziert)\.$"),
            Tuple.Create(GameLanguageEnum.English, @"^ ⇒ (?<block>Blocked! )?(?<parry>Parried! )?(?<crit>Critical! )?(?<target>You|.+) takes? (?<amount>\d+) ?(\((?<modifier>.\d+)%\) )?damage\.$"),
            Tuple.Create(GameLanguageEnum.France, @"^ ⇒ (?<parry>Parade ?! )?(?<block>Blocage ?! )?(?<crit>Critique ?! )?(?<target>Vous|.+) subi(t|ssez?)? (?<amount>\d+) ?(\((?<modifier>.\d+)%\) )?points? de dégâts?\.$"),
            Tuple.Create(GameLanguageEnum.Japanese, @"^ ⇒ (?<crit>クリティカル！ )?(?<target>.+)((に|は)、?)(?<block>ブロックした！ )?(?<parry>受け流した！ )?(?<amount>\d+) ?(\((?<modifier>.\d+)%\) ?)?ダメージ。$"),
            Tuple.Create(GameLanguageEnum.Chinese, @"^: ⇒ (?<crit>暴击！ )?(?<target>.+?)(?<block>招架住了！ )?(?<parry>格挡住了！ )?(受到(了)?)(?<amount>\d+) ?(\((?<modifier>.\d+)%\) ?)?点伤害。$"));

        /// <summary>
        /// Regex for parsing an auto-attack damage
        /// </summary>
        /// <returns>
        /// Regex Match Groups:
        /// block - contains text if there is a block
        /// parry - contains text if there is an parry
        /// crit - contains text if there is an critical hit
        /// direct - contains text if the hit was an direct hit
        /// source - name of the one doing the damage
        /// target - name of one receiving the damage
        /// amount - amount of damage
        /// modifier - how many percent modifier
        /// </returns>
        public readonly static RegExTypePair DamageMonsterAutoAttack = new RegExTypePair(SubjectMonster, null,
            Tuple.Create(GameLanguageEnum.German, @"^(?! ⇒)(?<block>Geblockt! ?)?(?<parry>Pariert! ?)?(?<crit>Kritischer Treffer! ?)?(D(u|einer|(i|e)r|ich|as|ie|en) )?(?<source>.+) triffs?t (?<target>dich|.+)( und verursachs?t |, aber der Schaden wird auf )(?<amount>\d+) ?(\((?<modifier>.\d+)%\) )?Punkte? (Schaden|reduziert)\.$"),
            Tuple.Create(GameLanguageEnum.English, @"^(?! ⇒)(?<block>Blocked! )?(?<parry>Parried! )?(?<crit>Critical! )?((T|t)he )?(?<source>.+) hits? (?<target>you|.+) for (?<amount>\d+) ?(\((?<modifier>.\d+)%\) )?damage\.$"),
            Tuple.Create(GameLanguageEnum.France, @"^(?! ⇒)(?<parry>Parade ?! )?(?<block>Blocage ?! )?(?<crit>Critique ?! )?(L[aes] |[LEAD]')?(?<source>.+) ((?<target>Vous|.+) infligez?|infligez? à (?<target>vous|.+)) (?<amount>\d+) ?(\((?<modifier>.\d+)%\) )?points? de dégâts?\.$"),
            Tuple.Create(GameLanguageEnum.Japanese, @"^(?! ⇒)(?<source>.+)の攻撃( ⇒ )?(?<crit>クリティカル！ )?(?<target>.+)((に|は)、?)(?<block>ブロックした！ )?(?<parry>受け流した！ )?(?<amount>\d+) ?(\((?<modifier>.\d+)%\) ?)?ダメージ。$"),
            Tuple.Create(GameLanguageEnum.Chinese, @"^:(?<source>.+)发动攻击( ⇒ )?(?<crit>暴击！ )?(?<target>.+?)(?<block>招架住了！ )?(?<parry>格挡住了！ )?(受到(了)?)(?<amount>\d+) ?(\((?<modifier>.\d+)%\) ?)?点伤害。$"));

        /// <summary>
        /// Regex for parsing an cure
        /// </summary>
        /// <returns>
        /// Regex Match Groups:
        /// crit - contains text if there is an critical hit
        /// target - name of one receiving the cure
        /// amount - amount of cure
        /// modifier - how many percent modifier
        /// </returns>
        public readonly static RegExTypePair CurePlayer = new RegExTypePair(SubjectPlayer, null,
                    Tuple.Create(GameLanguageEnum.German, @"^( ⇒ )?(?<crit>Kritischer Treffer ?! )?(D(u|einer|(i|e)r|ich|as|ie|en) )?(?<target>.+) regeneriers?t (?<amount>\d+) ?(\((?<modifier>.\d+)%\) )?(?<type>\w+)\.$"),
                    Tuple.Create(GameLanguageEnum.English, @"( ⇒ )?(?<crit>Critical! )?((T|t)he )?(?<target>You|.+) (recover|absorb)?s? (?<amount>\d+) ?(\((?<modifier>.\d+)%\) )?(?<type>\w+)\.$"),
                    Tuple.Create(GameLanguageEnum.France, @"^( ⇒ )?(?<crit>Critique ?! )?(?<target>Vous|.+) récup(é|è)rez? (?<amount>\d+) ?(\((?<modifier>.\d+)%\) )?(?<type>\w+)\.$"),
                    Tuple.Create(GameLanguageEnum.Japanese, @"^( ⇒ )?(?<crit>クリティカル！ )?(?<target>.+)((に|は)、?)(?<amount>\d+) ?(\((?<modifier>.\d+)%\) ?)?(?<type>\w+)回復。$"),
                    Tuple.Create(GameLanguageEnum.Chinese, @"^:( ⇒ )?(?<crit>暴击！ )?(?<target>You|.+)恢复了?(?<amount>\d+)?(\((?<modifier>.\d+)%\))?点(?<type>\w+)。$"));

        public readonly static RegExTypePair CurePlayerAction = new RegExTypePair(SubjectPlayer, null,
            Tuple.Create(GameLanguageEnum.English, @"^(?<target>.+)('s|r) (?<action>.*) restores (?<amount>\d+) of (her|his|your) HP\.$")
        );

        public readonly static RegExTypePair DetrimentalPlayer = new RegExTypePair(SubjectPlayer, null,
            // TODO: Find german translation Tuple.Create(GameLanguageEnum.German, @"^\.$"),
            Tuple.Create(GameLanguageEnum.English, @"^( ⇒ )?((T|t)he )?(?<target>You|.+) suffers? the effect of (?<status>.+)\.$"),
            Tuple.Create(GameLanguageEnum.France, @"^( ⇒ )?(?<target>Vous|.+) subi(t|ssez?) l'effet (?<status>.+)\.$"),
            Tuple.Create(GameLanguageEnum.Japanese, @"^( ⇒ )?(?<target>.+)((に|は)、?)「(?<status>.+)」の効果。$"),
            Tuple.Create(GameLanguageEnum.Chinese, @"^:( ⇒ )?对(?<target>.+)附加了“?(?<status>.+)”的效果。$")
        );

        public readonly static RegExTypePair DetrimentalPlayerRecovers = new RegExTypePair(SubjectPlayer, null,
            // TODO: Find german translation Tuple.Create(GameLanguageEnum.German, @"^\.$"),
            Tuple.Create(GameLanguageEnum.English, @"^( ⇒ )?((T|t)he )?(?<target>You|.+) recovers? from the effect of (?<status>.+)\.$"),
            Tuple.Create(GameLanguageEnum.France, @"^( ⇒ )?(?<target>Vous|.+) (perd(ez?)?|ne subi(t|ssez?)) plus l'effet (?<status>.+)\.$"),
            Tuple.Create(GameLanguageEnum.Japanese, @"^( ⇒ )?(?<target>.+)((に|は)、?)「(?<status>.+)」が切れた。$"),
            Tuple.Create(GameLanguageEnum.Chinese, @"^:( ⇒ )?(?<target>.+)的“(?<status>.+)”状态效果消失了。$")
        );

        public readonly static RegExTypePair BeneficialPlayer = new RegExTypePair(SubjectPlayer, null,
            Tuple.Create(GameLanguageEnum.German, @"^( ⇒ )?(D(u|einer|(i|e)r|ich|as|ie|en) )?(?<target>.+) erh lt(st| den) Effekt von (?<status>.+)\.$"),
            Tuple.Create(GameLanguageEnum.English, @"^( ⇒ )?(?<target>You|.+) gains? the effect of (?<status>.+)\.$"),
            Tuple.Create(GameLanguageEnum.France, @"^( ⇒ )?(?<target>Vous|.+) bénéficiez? de l'effet (?<status>.+)\.$"),
            Tuple.Create(GameLanguageEnum.Japanese, @"^( ⇒ )?(?<target>.+)((に|は)、?)「(?<status>.+)」の効果。$"),
            Tuple.Create(GameLanguageEnum.Chinese, @"^:( ⇒ )?对(?<target>You|.+)附加了“?(?<status>.+)”的效果。$")
        );

        public readonly static RegExTypePair BeneficialLosePlayer = new RegExTypePair(SubjectPlayer, null,
            // TODO: Find german translation for this, Tuple.Create(GameLanguageEnum.German, @"^\.$"),
            Tuple.Create(GameLanguageEnum.English, @"^( ⇒ )?(?<target>You|.+) loses? the effect of (?<status>.+)\.$"),
            Tuple.Create(GameLanguageEnum.France, @"^( ⇒ )?(?<target>Vous|.+) perd(ez?)? l'effet (?<status>.+)\.$"),
            Tuple.Create(GameLanguageEnum.Japanese, @"^( ⇒ )?(?<target>.+)((に|は)、?)「(?<status>.+)」が切れた。$"),
            Tuple.Create(GameLanguageEnum.Chinese, @"^:( ⇒ )?(?<target>You|.+)的“(?<status>.+)”状态效果消失了。$")
        );


        #region Misc chat lines
        public readonly static RegExTypePair MiscReadiesAction = new RegExTypePair(null, null, Tuple.Create(GameLanguageEnum.English, @"^(?<source>You|.+) readies (?<action>.+)\.$"));
        public readonly static RegExTypePair MiscBeginCasting = new RegExTypePair(null, null, Tuple.Create(GameLanguageEnum.English, @"^(?<source>You|.+) (begin)s? casting (?<action>.+)\.$"));
        public readonly static RegExTypePair MiscCancelAction = new RegExTypePair(null, null, Tuple.Create(GameLanguageEnum.English, @"^(?<source>You|.+) (cancel)s? (?<action>.+)\.$"));
        public readonly static RegExTypePair MiscInterruptedAction = new RegExTypePair(null, null, Tuple.Create(GameLanguageEnum.English, @"^(?<source>You|.+)('s|rs) (?<action>.+) is interrupted\.$"));
        public readonly static RegExTypePair MiscEnmityIncrease = new RegExTypePair(null, null, Tuple.Create(GameLanguageEnum.English, @"^ ⇒ (?<source>You|.+)('s|rs) enmity increases\.$"));
        public readonly static RegExTypePair MiscReadyTeleport = new RegExTypePair(null, null, Tuple.Create(GameLanguageEnum.English, @"^(?<source>You|.+) ready Teleport.$"));
        public readonly static RegExTypePair MiscMount = new RegExTypePair(null, null, Tuple.Create(GameLanguageEnum.English, @"^(?<source>You|.+) mount the (?<target>.+)\.$"));
        public readonly static RegExTypePair MiscTargetOutOfRange = new RegExTypePair(null, null, Tuple.Create(GameLanguageEnum.English, @"^Target out of range. (?<source>You|.+)'s (?<action>.+) was canceled\.$"));
        #endregion
    }
}