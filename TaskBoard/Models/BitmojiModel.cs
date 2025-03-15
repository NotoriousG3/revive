using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TaskBoard.Models;
public class BitmojiModel : IDisposable
{
    [Key] public long Id { get; set; }
    public string Name { get; set; }
    public int Gender { get; set; } 
    public int Style { get; set; } 
    public int Rotation { get; set; } 
    public int Body { get; set; } 
    public int Bottom { get; set; } 
    public int BottomTone1 { get; set; } 
    public int BottomTone2 { get; set; } 
    public int BottomTone3 { get; set; } 
    public int BottomTone4 { get; set; } 
    public int BottomTone5 { get; set; } 
    public int BottomTone6 { get; set; } 
    public int BottomTone7 { get; set; } 
    public int BottomTone8 { get; set; } 
    public int BottomTone9 { get; set; } 
    public int BottomTone10 { get; set; } 
    public int Brow { get; set; } 
    public int ClothingType { get; set; } 
    public int Ear { get; set; } 
    public int Eye { get; set; } 
    public int Eyelash { get; set; } 
    public int FaceProportion { get; set; } 
    public int Footwear { get; set; } 
    public int FootwearTone1 { get; set; } 
    public int FootwearTone2 { get; set; } 
    public int FootwearTone3 { get; set; } 
    public int FootwearTone4 { get; set; } 
    public int FootwearTone5 { get; set; } 
    public int FootwearTone6 { get; set; } 
    public int FootwearTone7 { get; set; } 
    public int FootwearTone8 { get; set; } 
    public int FootwearTone9 { get; set; } 
    public int FootwearTone10 { get; set; } 
    public int Hair { get; set; } 
    public int HairTone { get; set; } 
    public int IsTucked { get; set; } 
    public int Jaw { get; set; } 
    public int Mouth { get; set; } 
    public int Nose { get; set; } 
    public int Pupil { get; set; } 
    public int PupilTone { get; set; } 
    public int SkinTone { get; set; } 
    public int Sock { get; set; } 
    public int SockTone1 { get; set; } 
    public int SockTone2 { get; set; } 
    public int SockTone3 { get; set; } 
    public int SockTone4 { get; set; } 
    public int Top { get; set; } 
    public int TopTone1 { get; set; } 
    public int TopTone2 { get; set; } 
    public int TopTone3 { get; set; } 
    public int TopTone4 { get; set; } 
    public int TopTone5 { get; set; } 
    public int TopTone6 { get; set; } 
    public int TopTone7 { get; set; } 
    public int TopTone8 { get; set; } 
    public int TopTone9 { get; set; } 
    public int TopTone10 { get; set; }

    public void Dispose()
    {
        GC.SuppressFinalize(this);
    }
}