using UnityEngine;
using System.IO;
using System.Text;

public class FileManager : MonoBehaviour {

    public string fileName;

    private FileStream saveFile;
    private int startSize = 0;

    void Start()
    {
        openFile(fileName); 
    }

    public void saveProgress()
    {
        if(AI.instance.status == AI.Status.Random)
        {
            //Save this new individual in the file
        }
    }

    private void readFile(FileStream file)
    {

    }

    private void writeText(FileStream file, string text)
    {
        byte[] bText = new UTF8Encoding(true).GetBytes(text);
        file.Write(bText, 0, bText.Length);
    }

    private void openFile(string file)
    {
        if (File.Exists(file))
        {
            Debug.Log("File: " + file + " Found");
        }
        else
        {
            Debug.Log("File: " + file + " NOT Found");
            saveFile = File.Create(file);
        }
    }
}
