﻿namespace Microsoft.Azure.Devices.Applications.RemoteMonitoring.DeviceAdmin.Infrastructure.Exceptions
{
    public enum SecurityAction
    {

        //
        // 概要:
        //     スタック内の上位にあるすべての呼び出し元に、現在のアクセス許可オブジェクトによって指定されているアクセス許可が与えられている必要があります。詳細については、「[<topic://cpconmakingsecuritydemands>]」を参照してください。
        Demand = 2,
        //
        // 概要:
        //     スタック内の上位にある呼び出し元に現在のアクセス許可オブジェクトによって識別されているリソースへのアクセス許可がない場合でも、呼び出しコードがそのリソースにアクセスできます。詳細については、「[<topic://cpconassert>]」を参照してください。
        Assert = 3,
        //
        // 概要:
        //     呼び出し元に現在のアクセス許可オブジェクトによって指定されているリソースにアクセスする許可が与えられていても、そのリソースへの呼び出し元からのアクセスは拒否されます。詳細については、「[<topic://cpcondeny>]」を参照してください。
        Deny = 4,
        //
        // 概要:
        //     このアクセス許可オブジェクトによって指定されているリソースだけにアクセスできます。これ以外のリソースへのアクセス許可がコードに与えられている場合でも同様です。詳細については、「[<topic://cpconpermitonly>]」を参照してください。
        PermitOnly = 5,
        //
        // 概要:
        //     直前の呼び出し元に、指定したアクセス許可が与えられている必要があります。 .NET Framework 4 では使用しないでください。 完全な信頼がある場合は、System.Security.SecurityCriticalAttribute
        //     を使用します。部分信頼がある場合は、System.Security.Permissions.SecurityAction.Demand を使用します。
        LinkDemand = 6,
        //
        // 概要:
        //     クラスを継承する派生クラスやメソッドをオーバーライドする派生クラスに、指定したアクセス許可が与えられている必要があります。 詳細については、「継承確認要求」を参照してください。
        InheritanceDemand = 7,
        //
        // 概要:
        //     コードを実行するために必要な最低限のアクセス許可に対する要求。 このアクションは、アセンブリのスコープ内でだけ使用できます。
        RequestMinimum = 8,
        //
        // 概要:
        //     省略可能な (実行するために必須ではない) 追加のアクセス許可に対する要求。 この要求は、明確に要求されていない他のすべてのアクセス許可を暗黙で拒否します。
        //     このアクションは、アセンブリのスコープ内でだけ使用できます。
        RequestOptional = 9,
        //
        // 概要:
        //     不正使用される可能性があるアクセス許可を呼び出しコードに与えないようにする要求。 このアクションは、アセンブリのスコープ内でだけ使用できます。
        RequestRefuse = 10
    }
}