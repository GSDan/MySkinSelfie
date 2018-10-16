using System.Collections.Generic;

namespace SkinSelfie.AppModels
{
    public class TranslationDict
    {
        public string LanguageKey { get; set; }
        // <EnglishKey, Translated>
        public Dictionary<string, string> Translations { get; set; }
    }

    //TranslationDict es = new TranslationDict()
    //{
    //    LanguageKey = "es",
    //    Translations = new Dictionary<string, string>
    //            {
    //                {"Head","Cabeza"},
    //                {"Neck", "Cuello"},
    //                {"Torso", "Torso"},
    //                {"Arm", "Brazo"},
    //                {"Groin", "Ingle"},
    //                {"Buttock", "Nalga"},
    //                {"Leg", "Pierna"},
    //                {"Full Face", "Cara completa"},
    //                {"Left Side of Face", "Lado izquierdo de la cara"},
    //                {"Right Side of Face", "Lado derecho de la cara"},
    //                {"Back of Head", "Parte posterior de la cabeza"},
    //                {"Top of Head", "Arriba de la cabeza"},
    //                {"Front of Neck", "Frente del cuello"},
    //                {"Back of Neck", "Atrás del cuello" },
    //                {"Left Side of Neck", "Lado izquierdo del cuello" },
    //                {"Right Side of Neck", "Lado derecho del cuello"},
    //                {"Left Upper Arm", "Brazo superior izquierdo"},
    //                {"Left Forearm", "Antebrazo izquierdo"},
    //                {"Left Wrist", "Muñeca izquierda" },
    //                {"Left Hand", "Mano izquierda"},
    //                {"Right Upper Arm", "Brazo superior derecho" },
    //                {"Right Forearm", "Antebrazo derecho" },
    //                {"Right Wrist", "Muñeca derecha"},
    //                {"Right Hand", "Mano derecha" },
    //                {"Groin Area", "Área de la ingle"},
    //                {"Left Thigh", "Muslo izquierdo"},
    //                {"Left Back of Thigh", "Parte posterior del muslo izquierdo" },
    //                {"Left Knee", "Rodilla izquierda" },
    //                {"Left Shin", "Shin izquierdo"},
    //                {"Left Calf", "Pantorrilla izquierda" },
    //                {"Left Ankle", "Tobillo izquierdo"},
    //                {"Left Foot", "Pie izquierdo" },
    //                {"Right Thigh", "Muslo derecho" },
    //                {"Right Back of Thigh", "Parte posterior del muslo derecho" },
    //                {"Right Knee", "Rodilla derecha"},
    //                {"Right Shin", "Espinilla derecha" },
    //                {"Right Calf", "Pantorrilla derecha" },
    //                {"Right Ankle", "Tobillo derecho"},
    //                {"Right Foot", "Pie derecho"},
    //                {"Chest", "Pecho"},
    //                {"Stomach", "Estómago"},
    //                {"Upper Back", "Superior de la espalda" },
    //                {"Lower Back", "Espalda baja" },
    //                {"Left Side", "Lado izquierdo" },
    //                {"Left Shoulder", "Hombro izquierdo" },
    //                {"Left Shoulder Blade", "Omóplato izquierdo" },
    //                {"Right Side", "Lado derecho" },
    //                {"Right Shoulder", "Hombro derecho" },
    //                {"Right Shoulder Blade", "Omoplato derecho" },
    //                {"Left Buttock", "Nalga izquierda" },
    //                {"Right Buttock", "Nalgas derechas" }
    //            }
    //};
}
