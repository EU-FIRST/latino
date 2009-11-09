/*==========================================================================;
 *
 *  This file is part of LATINO. See http://latino.sf.net
 *
 *  File:          DanishStopWords.cs
 *  Version:       1.0
 *  Desc:		   Danish stop words
 *  Author:        Miha Grcar
 *  Created on:    Nov-2009
 *  Last modified: Nov-2009
 *  Revision:      Nov-2009
 *
 ***************************************************************************/

namespace Latino.TextMining
{
    /* .-----------------------------------------------------------------------
       |
       |  Static class StopWords
       |
       '-----------------------------------------------------------------------
    */
    public static partial class StopWords
    {
        // this list is taken from http://snowball.tartarus.org/algorithms/danish/stop.txt
        public static Set<string>.ReadOnly DanishStopWords
            = new Set<string>.ReadOnly(new Set<string>(new string[] {
                // A Danish stop word list. 
                // This is a ranked list (commonest to rarest) of stopwords derived from
                // a large text sample.
                "og",           // and
                "i",            // in
                "jeg",          // I
                "det",          // that (dem. pronoun)/it (pers. pronoun)
                "at",           // that (in front of a sentence)/to (with infinitive)
                "en",           // a/an
                "den",          // it (pers. pronoun)/that (dem. pronoun)
                "til",          // to/at/for/until/against/by/of/into, more
                "er",           // present tense of "to be"
                "som",          // who, as
                "p�",           // on/upon/in/on/at/to/after/of/with/for, on
                "de",           // they
                "med",          // with/by/in, along
                "han",          // he
                "af",           // of/by/from/off/for/in/with/on, off
                "for",          // at/for/to/from/by/of/ago, in front/before, because
                "ikke",         // not
                "der",          // who/which, there/those
                "var",          // past tense of "to be"
                "mig",          // me/myself
                "sig",          // oneself/himself/herself/itself/themselves
                "men",          // but
                "et",           // a/an/one, one (number), someone/somebody/one
                "har",          // present tense of "to have"
                "om",           // round/about/for/in/a, about/around/down, if
                "vi",           // we
                "min",          // my
                "havde",        // past tense of "to have"
                "ham",          // him
                "hun",          // she
                "nu",           // now
                "over",         // over/above/across/by/beyond/past/on/about, over/past
                "da",           // then, when/as/since
                "fra",          // from/off/since, off, since
                "du",           // you
                "ud",           // out
                "sin",          // his/her/its/one's
                "dem",          // them
                "os",           // us/ourselves
                "op",           // up
                "man",          // you/one
                "hans",         // his
                "hvor",         // where
                "eller",        // or
                "hvad",         // what
                "skal",         // must/shall etc.
                "selv",         // myself/youself/herself/ourselves etc., even
                "her",          // here
                "alle",         // all/everyone/everybody etc.
                "vil",          // will (verb)
                "blev",         // past tense of "to stay/to remain/to get/to become"
                "kunne",        // could
                "ind",          // in
                "n�r",          // when
                "v�re",         // present tense of "to be"
                "dog",          // however/yet/after all
                "noget",        // something
                "ville",        // would
                "jo",           // you know/you see (adv), yes
                "deres",        // their/theirs
                "efter",        // after/behind/according to/for/by/from, later/afterwards
                "ned",          // down
                "skulle",       // should
                "denne",        // this
                "end",          // than
                "dette",        // this
                "mit",          // my/mine
                "ogs�",         // also
                "under",        // under/beneath/below/during, below/underneath
                "have",         // have
                "dig",          // you
                "anden",        // other
                "hende",        // her
                "mine",         // my
                "alt",          // everything
                "meget",        // much/very, plenty of
                "sit",          // his, her, its, one's
                "sine",         // his, her, its, one's
                "vor",          // our
                "mod",          // against
                "disse",        // these
                "hvis",         // if
                "din",          // your/yours
                "nogle",        // some
                "hos",          // by/at
                "blive",        // be/become
                "mange",        // many
                "ad",           // by/through
                "bliver",       // present tense of "to be/to become"
                "hendes",       // her/hers
                "v�ret",        // be
                "thi",          // for (conj)
                "jer",          // you
                "s�dan"}));     // such, like this/like that    
    }
}
