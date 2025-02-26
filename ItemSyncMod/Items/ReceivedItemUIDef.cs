﻿using ItemChanger;
using ItemChanger.UIDefs;
using ItemSyncMod.Items.DisplayMessageFormatter;

namespace ItemSyncMod.Items
{
    public class ReceivedItemUIDef : MsgUIDef
    {
        public static UIDef Convert(UIDef orig, string from, IDisplayMessageFormatter formatter)
        {
            if (orig is MsgUIDef msgDef)
                return new ReceivedItemUIDef(msgDef, from, formatter);

            return orig;
        }

        public ReceivedItemUIDef(MsgUIDef msgUIDef, string from, IDisplayMessageFormatter formatter)
        {
            this.msgUIDef = msgUIDef;
            From = from;
            Formatter = formatter;

            name = this.msgUIDef?.name?.Clone();
            shopDesc = this.msgUIDef?.shopDesc?.Clone();
            sprite = this.msgUIDef?.sprite?.Clone();
            if (ItemSyncMod.RecentItemsInstalled)
                AddRecentItemsTagCallback();
        }

        private MsgUIDef msgUIDef;
        internal string From { get; private set; }
        internal IDisplayMessageFormatter Formatter { get; private set; }

        private void AddRecentItemsTagCallback()
        {
            RecentItemsDisplay.Events.ModifyDisplayItem += this.AddRecentItemsTag;
        }

        public override void SendMessage(MessageType type, Action callback)
        {
            var tmp = name;
            switch (ItemSyncMod.GS.CornerMessagePreference)
            {
                case GlobalSettings.InfoPreference.Both:
                    name = new BoxedString(Formatter.GetCornerMessage(GetPostviewName(), From));
                    break;
            }
            base.SendMessage(type, callback);
            name = tmp;
        }
    }

    internal static class ReceivedItemUIDefExtensions
    {
        internal static void AddRecentItemsTag(this ReceivedItemUIDef self, RecentItemsDisplay.ItemDisplayArgs args)
        {
            try
            {
                switch (ItemSyncMod.GS.RecentItemsPreference)
                {
                    case GlobalSettings.InfoPreference.SenderOnly:
                    case GlobalSettings.InfoPreference.Both:
                        args.DisplayMessage = self.Formatter.GetDisplayMessage(args.DisplayName,
                            self.From, args.DisplaySource, ItemSyncMod.GS.RecentItemsPreference);
                        break;
                }
            }
            catch (Exception ex)
            {
                LogHelper.LogError($"Exception during formatter.GetDisplayMessage of item {self.name}," +
                    $" displayed as {args.DisplayName} from {self.From}, source {args.DisplaySource}\n{ex}");
            }

            RecentItemsDisplay.Events.ModifyDisplayItem -= self.AddRecentItemsTag;
        }


    }
}
