using UnityEngine;
using System.IO;
using System.Text;
using System.Collections.Generic;

public class FileManager : MonoBehaviour {

    public string fileName;

    private StreamWriter writer;
    private StreamReader reader;

    public static FileManager instance;

    void Awake()
    {
        //Singleton
        if (instance != null)
        {
            Debug.LogError("More than one FileManager in scene!");
            return;
        }
        instance = this;
    }

    void Start()
    {
        OpenFile(fileName); 
    }

    public void SaveProgress(float fittness)
    {
        if(AI.instance.status == AI.Status.Random)
        {
            //Save this new individual in the file
            List<AI.TurrPoint> turrets = AI.instance.placedTurrets;
            string nodeText;
            string totalText = "";
            for(int i = 0; i < turrets.Count; ++i)
            {
                //TODO: add zero padding
                nodeText = "N" + turrets[i].P().X() + "x" + turrets[i].P().Y() + "-"+ turrets[i].Fittness();
                totalText += nodeText;
            }
            //TODO: add zero padding
            totalText += "E" + fittness;
            WriteText(totalText);
        }
        else if (AI.instance.status == AI.Status.Learn)
        {
            //ReCombine these new individuals with old individuals

            //Mutate some of the individuals
        }
    }

    public string ReadLine(FileStream file)
    {
        return reader.ReadLine();
    }

    public void WriteText(string text)
    {
        writer.WriteLine(text);
    }

    private void OpenFile(string file)
    {
        if (File.Exists(file))
        {
            Debug.Log("File: " + file + " Found");
        }
        else
        {
            Debug.Log("File: " + file + " NOT Found");
            File.Create(file);
        }

        //Create writer for file, append text not overwrite
        if(AI.instance.status == AI.Status.Random)
        {
            writer = new StreamWriter(file, true);
            reader = null;
        }
        else
        {
            reader = new StreamReader(file);
            writer = null;
        }
        
        
    }

    void OnApplicationQuit()
    {
        Debug.Log("Application ending after " + Time.time + " seconds");
        if(writer !=null)
            writer.Close();
        if (reader != null)
            reader.Close();
    }
}
