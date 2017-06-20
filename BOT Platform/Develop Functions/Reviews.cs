
using System;
using BOT_Platform;
using VkNet.Model;
using VkNet.Model.RequestParams;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace MyFunctions
{
    //class ReviewManager : IMyCommands
    //{
        /*
        const string DATA_FILE = "Data\\reviews.txt";
        class Review
        {
            public ulong userId { get; private set; }
            public string review { get; private set; }

            public Review(ulong id, string r)
            {
                userId = id;
                review = r;
            }
        }

        static SortedDictionary<ulong, Review> Reviews;

        public void AddMyCommandInPlatform()
        {
            CommandsList.TryAddCommand("отзыв", new MyComandStruct("", AddReview));
        }

        private void AddReview(Message message, object[] p)
        {
            if (string.IsNullOrEmpty(p.ToString())) ; 
        }

        public bool NeedCommandInfo(Message message, params object[] p)
        {
            return true;
        }

        public ReviewManager()
        {
            AddMyCommandInPlatform();

            FileInfo data = new FileInfo(DATA_FILE);
            if (!data.Exists)
            {
                File.Create(DATA_FILE);
            }

            //Deserialize();
            //if (Reviews == null) Reviews = new SortedDictionary<string, Review>();
        }

        static void Serialize()
        {
            FileStream fs = new FileStream(DATA_FILE, FileMode.Create);

            BinaryFormatter formatter = new BinaryFormatter();
            try
            {
                formatter.Serialize(fs, Reviews);
            }
            catch (SerializationException e)
            {
                Console.WriteLine("[SYSTEM][ERROR]:\n" +
                                  "Ошибка при сериализации отзывов: " + e.Message);
            }
            finally
            {
                fs.Close();
            }
        }

        static void Deserialize()
        {

            FileStream fs = new FileStream(DATA_FILE, FileMode.Open);
            try
            {
                BinaryFormatter formatter = new BinaryFormatter();

                if (Reviews != null) Reviews.Clear();
                //Reviews = (SortedDictionary<string, Review>)formatter.Deserialize(fs);
            }
            catch (SerializationException e)
            {
                Console.WriteLine("[SYSTEM][ERROR]:\n" +
                                  "Ошибка при десериализации отзывов: " + e.Message);
            }
            finally
            {
                fs.Close();
            }

        }*/
    //}
}
