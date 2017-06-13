using System;
using System.Data;
using EVE_Killboard_Analyser.Models;
using log4net;

namespace EVE_Killboard_Analyser.Helper.DatabaseWriter
{
    //public class CharacterV1DataWriter : DatabaseWriter<CharacterV1DataEntry>
    //{
    //    private static readonly CharacterV1DataWriter INSTANCE = new CharacterV1DataWriter();
    //    private static readonly ILog LOG = LogManager.GetLogger(typeof(CharacterV1DataWriter));

    //    private CharacterV1DataWriter()
    //    {
    //        Start();
    //    }

    //    public static CharacterV1DataWriter Instance { get { return INSTANCE; } }

    //    protected override int MaxEntryCount
    //    {
    //        get { return 500; }
    //    }

    //    protected override void WriteNextEntryToDatabase()
    //    {
    //        var data = WriteQueue.Take();
    //        //LOG.Debug(string.Format("{0}: writing analysis result to database", data.CharacterID));
    //        var start = DateTime.UtcNow;
    //        try
    //        {
    //            InsertOrUpdate(data.CharacterID, data, context => context.AnalysisResults);
                
    //            var elapsedSeconds = (DateTime.UtcNow - start).TotalSeconds;
    //            LOG.Debug(string.Format("{0}: analysis result successfully written in {1}s", data.CharacterID,
    //                                    elapsedSeconds));
    //        }
    //        catch (Exception e)
    //        {
    //            LOG.Error(string.Format("{0}: error during write of character data entry", data.CharacterID), e);
    //        }
    //    }
    //}
}