using System;
using System.IO;
using UnityEngine;

public class FileDataHandler 
{
    private string fullPath;
    private bool encryptData;
    private string codeWord = "multimedia.com";

    public FileDataHandler(string dataDirPath, string dataFileName, bool encryptData)
    {
        fullPath = Path.Combine(dataDirPath, dataFileName);
        this.encryptData = encryptData;
    }

    public void SaveData(GameData gameData)
    {
        try
        {
            Directory.CreateDirectory(Path.GetDirectoryName(fullPath));

            string dataToSave = JsonUtility.ToJson(gameData, true);

            if(encryptData)
                dataToSave=EncryptDecrypt(dataToSave);

            using (FileStream stream=new FileStream(fullPath,FileMode.Create))
            {
                using (StreamWriter write = new StreamWriter(stream))
                {
                    write.Write(dataToSave);
                }
            }
        }

        catch (Exception e)
        {
            Debug.LogError("Error on trying to save data to file;" + fullPath + "\n" + e);
        }
    }

    public GameData LoadData()
    {
        GameData loadData = null;
        //1.確認存檔存在
        if(File.Exists(fullPath))
        {
            try
            {
                string dataToLoad = "";
                //2.開啟檔案
                using (FileStream stream = new FileStream(fullPath, FileMode.Open))
                {
                    //3.閱讀檔案內容
                    using (StreamReader reader = new StreamReader(stream))
                    {
                        dataToLoad = reader.ReadToEnd();
                    }
                }

                if(encryptData)
                    dataToLoad =EncryptDecrypt(dataToLoad);
                //4.將Json檔回傳進GameData物件
                loadData = JsonUtility.FromJson<GameData>(dataToLoad);
            }

            catch (Exception e)
            {
                Debug.LogError("Error on trying to load data from file:" + fullPath + "\n" + e);
            }
        }
        return loadData;
    }

    public void Delete()
    {
        if(File.Exists(fullPath))
            File.Delete(fullPath);
    }

    private string EncryptDecrypt(string data)
    {
        string modifedData = "";

        for (int i = 0; i < data.Length; i++)
        {
            modifedData += (char)(data[i] ^ codeWord[i%codeWord.Length]);
        }

        return modifedData;
    }
}
