﻿/*==========================================================================;
 *
 *  This file is part of LATINO. See http://latino.sf.net
 *
 *  File:    GermanStopWords.cs
 *  Desc:    German stop words
 *  Created: Dec-2008
 *
 *  Authors: Miha Grcar
 *
 ***************************************************************************/

namespace Latino.TextMining
{
    /* .-----------------------------------------------------------------------
       |
       |  Class StopWords
       |
       '-----------------------------------------------------------------------
    */
    public static partial class StopWords
    {
        // this list is taken from http://snowball.tartarus.org/algorithms/german/stop.txt
        public static Set<string>.ReadOnly GermanStopWords
            = new Set<string>.ReadOnly(new Set<string>(new string[] {
                // A German stop word list. 
                // The number of forms in this list is reduced significantly by passing it
                // through the German stemmer.
                "aber",           //  but
                "alle",           //  all
                "allem",
                "allen",
                "aller",
                "alles",
                "als",            //  than, as
                "also",           //  so
                "am",             //  an + dem
                "an",             //  at
                "ander",          //  other
                "andere",
                "anderem",
                "anderen",
                "anderer",
                "anderes",
                "anderm",
                "andern",
                "anderr",
                "anders",
                "auch",           //  also
                "auf",            //  on
                "aus",            //  out of
                "bei",            //  by
                "bin",            //  am
                "bis",            //  until
                "bist",           //  art
                "da",             //  there
                "damit",          //  with it
                "dann",           //  then
                "der",            //  the
                "den",
                "des",
                "dem",
                "die",
                "das",
                "daß",            //  that
                "derselbe",       //  the same
                "derselben",
                "denselben",
                "desselben",
                "demselben",
                "dieselbe",
                "dieselben",
                "dasselbe",
                "dazu",           //  to that
                "dein",           //  thy
                "deine",
                "deinem",
                "deinen",
                "deiner",
                "deines",
                "denn",           //  because
                "derer",          //  of those
                "dessen",         //  of him
                "dich",           //  thee
                "dir",            //  to thee
                "du",             //  thou
                "dies",           //  this
                "diese",
                "diesem",
                "diesen",
                "dieser",
                "dieses",
                "doch",           //  (several meanings)
                "dort",           //  (over) there
                "durch",          //  through
                "ein",            //  a
                "eine",
                "einem",
                "einen",
                "einer",
                "eines",
                "einig",          //  some
                "einige",
                "einigem",
                "einigen",
                "einiger",
                "einiges",
                "einmal",         //  once
                "er",             //  he
                "ihn",            //  him
                "ihm",            //  to him
                "es",             //  it
                "etwas",          //  something
                "euer",           //  your
                "eure",
                "eurem",
                "euren",
                "eurer",
                "eures",
                "für",            //  for
                "fuer",
                "gegen",          //  towards
                "gewesen",        //  p.p. of sein
                "hab",            //  have
                "habe",           //  have
                "haben",          //  have
                "hat",            //  has
                "hatte",          //  had
                "hatten",         //  had
                "hier",           //  here
                "hin",            //  there
                "hinter",         //  behind
                "ich",            //  I
                "mich",           //  me
                "mir",            //  to me
                "ihr",            //  you, to her
                "ihre",
                "ihrem",
                "ihren",
                "ihrer",
                "ihres",
                "euch",           //  to you
                "im",             //  in + dem
                "in",             //  in
                "indem",          //  while
                "ins",            //  in + das
                "ist",            //  is
                "jede",           //  each, every
                "jedem",
                "jeden",
                "jeder",
                "jedes",
                "jene",           //  that
                "jenem",
                "jenen",
                "jener",
                "jenes",
                "jetzt",          //  now
                "kann",           //  can
                "kein",           //  no
                "keine",
                "keinem",
                "keinen",
                "keiner",
                "keines",
                "können",         //  can
                "koennen",
                "könnte",         //  could
                "koennte",
                "machen",         //  do
                "man",            //  one
                "manche",         //  some, many a
                "manchem",
                "manchen",
                "mancher",
                "manches",
                "mein",           //  my
                "meine",
                "meinem",
                "meinen",
                "meiner",
                "meines",
                "mit",            //  with
                "muss",           //  must
                "musste",         //  had to
                "nach",           //  to(wards)
                "nicht",          //  not
                "nichts",         //  nothing
                "noch",           //  still, yet
                "nun",            //  now
                "nur",            //  only
                "ob",             //  whether
                "oder",           //  or
                "ohne",           //  without
                "sehr",           //  very
                "sein",           //  his
                "seine",
                "seinem",
                "seinen",
                "seiner",
                "seines",
                "selbst",         //  self
                "sich",           //  herself
                "sie",            //  they, she
                "ihnen",          //  to them
                "sind",           //  are
                "so",             //  so
                "solche",         //  such
                "solchem",
                "solchen",
                "solcher",
                "solches",
                "soll",           //  shall
                "sollte",         //  should
                "sondern",        //  but
                "sonst",          //  else
                "über",           //  over
                "ueber",
                "um",             //  about, around
                "und",            //  and
                "uns",            //  us
                "unse",
                "unsem",
                "unsen",
                "unser",
                "unses",
                "unter",          //  under
                "viel",           //  much
                "vom",            //  von + dem
                "von",            //  from
                "vor",            //  before
                "während",        //  while
                "waehrend",
                "war",            //  was
                "waren",          //  were
                "warst",          //  wast
                "was",            //  what
                "weg",            //  away, off
                "weil",           //  because
                "weiter",         //  further
                "welche",         //  which
                "welchem",
                "welchen",
                "welcher",
                "welches",
                "wenn",           //  when
                "werde",          //  will
                "werden",         //  will
                "wie",            //  how
                "wieder",         //  again
                "will",           //  want
                "wir",            //  we
                "wird",           //  will
                "wirst",          //  willst
                "wo",             //  where
                "wollen",         //  want
                "wollte",         //  wanted
                "würde",          //  would
                "wuerde",
                "würden",         //  would
                "wuerden",
                "zu",             //  to
                "zum",            //  zu + dem
                "zur",            //  zu + der
                "zwar",           //  indeed
                "zwischen"}));    //  between
    }
}
