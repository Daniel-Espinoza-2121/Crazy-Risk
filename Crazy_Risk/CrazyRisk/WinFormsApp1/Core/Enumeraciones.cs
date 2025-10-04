using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CrazyRisk.Core
{
    public enum ETipoTarjeta
    {
        Infanteria,
        Caballeria,
        Artilleria,
    }

    public enum EstadoJuego
    {
        Preparacion,
        DistribucionInicial,
        Refuerzos,
        Ataque,
        Planeacion,
        Finalizado,
    }

    public enum EColorJugador
    {
        Rojo,
        Azul,
        Verde,
        Amarillo,
        Morado,
        Neutral
    }

    public enum TipoTrioTarjetas
    {
        Diferentes,        
        TresInfanteria,    
        TresCaballeria,    
        TresArtilleria     
    }
}
