public class ResultMsgLog
{
    public int Id { get; set; }
    public DtoGetUser WhoDone { get; set; }
    public DtoGetMessage WhatDone { get; set; }
    public string LogAction { get; set; }
    public string CreateDate { get; set; }
    public string CreateTime { get; set; }
}