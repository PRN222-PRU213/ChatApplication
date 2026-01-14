using System.Collections.ObjectModel;

namespace ChatApplication
{
    /// <summary>
    /// Cung cấp bộ emoji đầy đủ cho ứng dụng chat
    /// </summary>
    public static class EmojiProvider
    {
        // 😊 Mặt cười & Cảm xúc
        public static readonly string[] Smileys =
        [
            "😀", "😃", "😄", "😁", "😆", "😅", "🤣", "😂", "🙂", "😊",
            "😇", "🥰", "😍", "🤩", "😘", "😗", "😚", "😙", "🥲", "😋",
            "😛", "😜", "🤪", "😝", "🤑", "🤗", "🤭", "🤫", "🤔", "🤐"
        ];

        // 😢 Buồn & Tiêu cực
        public static readonly string[] Sad =
        [
            "😐", "😑", "😶", "😏", "😒", "🙄", "😬", "😮‍💨", "🤥", "😌",
            "😔", "😪", "🤤", "😴", "😷", "🤒", "🤕", "🤢", "🤮", "🥴",
            "😵", "🤯", "🥵", "🥶", "😱", "😨", "😰", "😥", "😢", "😭"
        ];

        // 😎 Cool & Đặc biệt
        public static readonly string[] Cool =
        [
            "😤", "😠", "😡", "🤬", "😈", "👿", "💀", "☠️", "💩", "🤡",
            "👹", "👺", "👻", "👽", "👾", "🤖", "😺", "😸", "😹", "😻",
            "😼", "😽", "🙀", "😿", "😾", "🙈", "🙉", "🙊", "😎", "🤓"
        ];

        // 👋 Tay & Cử chỉ
        public static readonly string[] Gestures =
        [
            "👋", "🤚", "🖐️", "✋", "🖖", "👌", "🤌", "🤏", "✌️", "🤞",
            "🤟", "🤘", "🤙", "👈", "👉", "👆", "🖕", "👇", "☝️", "👍",
            "👎", "✊", "👊", "🤛", "🤜", "👏", "🙌", "👐", "🤲", "🤝",
            "🙏", "✍️", "💅", "🤳", "💪", "🦾", "🦿", "🦵", "🦶", "👂"
        ];

        // ❤️ Tim & Tình yêu
        public static readonly string[] Hearts =
        [
            "❤️", "🧡", "💛", "💚", "💙", "💜", "🖤", "🤍", "🤎", "💔",
            "❣️", "💕", "💞", "💓", "💗", "💖", "💘", "💝", "💟", "♥️"
        ];

        // 🎉 Lễ hội & Hoạt động
        public static readonly string[] Activities =
        [
            "🎉", "🎊", "🎈", "🎁", "🎀", "🏆", "🥇", "🥈", "🥉", "⚽",
            "🏀", "🎮", "🎲", "🎯", "🎭", "🎨", "🎬", "🎤", "🎧", "🎵"
        ];

        // 🍔 Đồ ăn & Thức uống
        public static readonly string[] FoodAndDrink =
        [
            "🍕", "🍔", "🍟", "🌭", "🍿", "🧀", "🥚", "🍳", "🥞", "🧇",
            "🍞", "🥐", "🥖", "🥨", "🍩", "🍪", "🎂", "🍰", "🧁", "🍫",
            "☕", "🍵", "🧃", "🥤", "🍺", "🍻", "🥂", "🍷", "🍸", "🍹"
        ];

        // 🌟 Thiên nhiên & Thời tiết
        public static readonly string[] Nature =
        [
            "⭐", "🌟", "✨", "💫", "🌈", "☀️", "🌤️", "⛅", "🌥️", "☁️",
            "🌧️", "⛈️", "🌩️", "❄️", "🔥", "💧", "🌊", "🌸", "🌺", "🌻"
        ];

        // ✅ Biểu tượng & Ký hiệu
        public static readonly string[] Symbols =
        [
            "✅", "❌", "⭕", "❗", "❓", "💯", "🔴", "🟠", "🟡", "🟢",
            "🔵", "🟣", "⚫", "⚪", "🟤", "▶️", "⏸️", "⏹️", "🔊", "🔇"
        ];

        /// <summary>
        /// Lấy tất cả emoji
        /// </summary>
        public static ObservableCollection<string> GetAllEmojis()
        {
            var allEmojis = new ObservableCollection<string>();

            foreach (var emoji in Smileys) allEmojis.Add(emoji);
            foreach (var emoji in Sad) allEmojis.Add(emoji);
            foreach (var emoji in Cool) allEmojis.Add(emoji);
            foreach (var emoji in Gestures) allEmojis.Add(emoji);
            foreach (var emoji in Hearts) allEmojis.Add(emoji);
            foreach (var emoji in Activities) allEmojis.Add(emoji);
            foreach (var emoji in FoodAndDrink) allEmojis.Add(emoji);
            foreach (var emoji in Nature) allEmojis.Add(emoji);
            foreach (var emoji in Symbols) allEmojis.Add(emoji);

            return allEmojis;
        }

        /// <summary>
        /// Lấy emoji phổ biến (rút gọn)
        /// </summary>
        public static ObservableCollection<string> GetPopularEmojis()
        {
            return
            [
                "😀", "😂", "😍", "🥰", "😘", "😎", "🤔", "😢", "😭", "😱",
                "👍", "👎", "👏", "🙏", "💪", "✌️", "👋", "🤝", "✊", "👊",
                "❤️", "💔", "💕", "🔥", "✨", "⭐", "💯", "🎉", "🎁", "🏆",
                "✅", "❌", "❗", "❓", "👻", "💀", "😈", "🤖", "👽", "🤡"
            ];
        }

        /// <summary>
        /// Lấy emoji theo danh mục
        /// </summary>
        public static ObservableCollection<EmojiCategory> GetEmojisByCategory()
        {
            return
            [
                new EmojiCategory("😊 Cảm xúc", Smileys),
                new EmojiCategory("😢 Buồn", Sad),
                new EmojiCategory("😎 Cool", Cool),
                new EmojiCategory("👋 Cử chỉ", Gestures),
                new EmojiCategory("❤️ Tim", Hearts),
                new EmojiCategory("🎉 Hoạt động", Activities),
                new EmojiCategory("🍔 Đồ ăn", FoodAndDrink),
                new EmojiCategory("🌟 Thiên nhiên", Nature),
                new EmojiCategory("✅ Ký hiệu", Symbols)
            ];
        }
    }

    /// <summary>
    /// Danh mục emoji
    /// </summary>
    public class EmojiCategory
    {
        public string Name { get; }
        public string[] Emojis { get; }

        public EmojiCategory(string name, string[] emojis)
        {
            Name = name;
            Emojis = emojis;
        }
    }
}