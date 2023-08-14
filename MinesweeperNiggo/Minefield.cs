using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MinesweeperNiggo
{
    //Eine Kachel kann die folgenden Zustände haben: unbekannt, leer, Mine(n) als Nachbarn, Mine, Mit Flagge
    //Beim Klicken auf eine Kachel wird je nach Zustand ein anderes Verhalten ausgeführt
    public struct Tile
    {
        public bool mine;
        public int numberOfAdjacentMines; //bei Anzahl 0 wird nichts angezeigt
        public bool flagged;
        public bool clicked;
        //private int AnzahlMinen; --> dieses Attribut wird aktiviert wenn die Anzahl der Minen einstellbar wird


        //Konstruktor; ein Struct hat immer einen vollständigen Konstruktor. Jede Kachel bekommt aber beim Erstellen trotzdem nur seine STandardwerte
        public Tile(bool mine, int noam, bool flagged, bool clicked)
        {
            this.mine = false;
            numberOfAdjacentMines = 0;
            this.flagged = false;
            this.clicked = false;
        }
    }




    internal class Minefield
    {
        //Feldbeschreibung (Länge, Breite)
        public Tile[,] field;
        
        //Wird gesetzt wenn das Spiel gewonnen ist (alle Felder bis auf die Minen "clicked"
        public bool win = false;

        //wird das auf true gesetzt ist man auf eine Mine getreten und das Spiel endet, per default mit Wert false. wird gesetzt wenn eine Mine geklickt wurde
        public bool hitMine = false;

        //Konstruktor
        public Minefield(int i, int j) //das Feld wird als i x j Matrix erstellt
        {
            field = new Tile[i, j];
            initializeField(10); //vorerst wird es 10 Minen fix geben, variierende Anzahl wenn Schwierigkeitsgrad erstellt wird
        }



        //------------------Methoden------------------------------------------------

        //platziert Minen auf einem bestehenden Feld (wird in der nächsten Methode verwendet)
        private void placeMines(int count)
        {
            Random r = new Random();

            int c = 0; //counter

            while (c <= count)
            {
                int i = r.Next(0, field.GetLength(0)); 
                int j = r.Next(0, field.GetLength(1));

                if (!field[i,j].mine)
                {
                    field[i,j].mine = true;
                    c++;
                }
            }
        }




        //Das Zählen angrenzender Minen erfolgt für jedes Feld separat. Die Methode gilt für ein einziges Feld. Sie 
        //wird in einer doppelten Schleife aufgerufen eben für jedes Feld
        private void setNumberOfAdjacentMines(int i, int j)
        {
            int countAdjacentMines = 0;

            //Das Feld selbst darf keine Mine sein, dann ist nämlich die Anzahl benachbarter Minen irrelvant
            if (field[i, j].mine)
            {
                return;
            }


            //benötigt werde Felder im Bereich i-1 bis i+1 sowie j-1 bis j+1, außer der Fall (i,j) selbst
            //Beachte den Fall des Randfelds, also i-1 < 0, i+1 >= field.GetLength(0), analog j, diese Fälle müssen ausgeschlossen werden
            for (int m = i-1; m <= i+1; m++) //Zeilenindex
            {
                if (m < 0 || m >= field.GetLength(0)) 
                {
                    continue; //Schritt überspringen wenn über die Array-Grenze geprüft wird (Randfeld)
                }

                for (int n  = j-1; n <= j+1; n++) //Spaltenindex
                {
                    if (n < 0 || n >= field.GetLength(1))
                    {
                        continue; //Schritt überspringen wenn über die Array-Grenze geprüft wird (Randfeld)
                    }

                    if (m == i && n == j) //Das Feld ij selbst aus des Zählung ausschließen
                    {
                        continue;
                    }

                    if (field[m, n].mine)
                    {
                        countAdjacentMines++;
                    }
                }

            }


            field[i,j].numberOfAdjacentMines = countAdjacentMines;
        }





        private void initializeField(int numberOfMines)
        {
            //verteilen der Minen, bevor alles andere getan wird. Die Minen werden im nächsten Schritt gebraucht, da die 
            //leeren Felder möglicherweise die Anzahl der angrenzenden Minen angeben müssen
            placeMines(numberOfMines);

            //Leere Felder und Felder mit Zahlwerten erstellen (also die restlichen Felder)
            for (int i = 0; i <  field.GetLength(0); i++)
            {
                for (int j = 0; j < field.GetLength(1); j++)
                {
                    if (!field[i,j].mine)
                    {
                        setNumberOfAdjacentMines(i, j);
                    }
                }
            }
        }


        public void ClickTile(int i, int j)
        {
            if (i < 0 || j < 0 || i >= field.GetLength(0) || j >= field.GetLength(1)) //Array-Randüberschreitung --> Array-Zugriff verhindern (Rekursion unten)
            {
                return;
            }

            if (field[i, j].clicked || field[i,j].flagged) //nichts tun wenn das Feld bereits bearbeitet wurde oder  eine Flagge gesetzt wurde (wegen dem rekursiven Aufruf unten)
            {
                return;
            }

            if (field[i, j].mine) //auf eine Mine getreten
            {
                hitMine = true;
                showAllMines();
                return;
            }
            else
            {
                if (field[i,j].numberOfAdjacentMines != 0) //hat mindestens 1 benachbartes Minenfeld, also wird eine Zahl angezeigt die 1 oder größer ist
                {
                    field[i,j].clicked = true; 
                }
                else  //keine Minen in unmittelbarer Nachbarschaft, es werden rekursiv nachbarfelder angeklickt !
                {
                    field[i, j].clicked = true;

                    //rekursiver Aufrauf der Methode für alle Nachbarfelder
                    ClickTile(i - 1, j - 1);
                    ClickTile(i, j - 1);
                    ClickTile(i + 1, j - 1);

                    ClickTile(i - 1, j);
                    //ClickTile(i, j);   der hier wird ausgelassen, da es das bereits aufgerufene Feld ist
                    ClickTile(i + 1,j);

                    ClickTile(i - 1, j + 1);
                    ClickTile(i, j + 1);
                    ClickTile(i + 1, j + 1);

                }
            }

            isWon(); //prüft ob der Zustand "gewonnen" nun besteht

        }


        //durch erneutes klicken mit der rechten Maustaste kann die Flagge wieder entzogen (bzw neu gesetzt) werden
        public void setFlag(int i, int j)
        {
            field[i, j].flagged = !field[i,j].flagged;
        }



        //setzt alle Minenfelder auf clicked, im wpf-Fenster werden dann per refresh alle clicked-Felder angezeigt, aber auch alle 
        //buttons deaktiviert, da das Spiel verloren ist.
        private void showAllMines()
        {
            for (int i = 0; i < field.GetLength(0); i++)
            {
                for (int j = 0; j < field.GetLength(1); j++)
                {
                    if (field[i, j].mine)
                    {
                        field[i, j].clicked = true;
                    }
                }            
            }
        }

        private void isWon()
        {
            foreach(var item  in field)
            {
                if (!item.clicked && !item.mine)
                {
                    return;
                }
            }

            win = true; //wird nur erreicht, wenn jedes Feld das keine Mine ist geklickt wurde
        }
    }
}
