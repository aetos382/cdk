using Amazon.CDK;
using Amazon.CDK.AWS.EC2;
using Amazon.CDK.AWS.ECS;
using Amazon.CDK.AWS.ElasticLoadBalancingV2;

#if CDK_V2
using Constructs;
#endif

namespace Cdk
{
    public class CdkStack :
        Stack
    {
        internal CdkStack(
            Construct scope,
            string id,
            IStackProps props = null)
            : base(
                  scope,
                  id,
                  props)
        {
            var vpc = new Vpc(this, "Vpc", new VpcProps
            {
            });

            var ecsCluster = new Cluster(this, "EcsCluster", new ClusterProps
            {
                Vpc = vpc
            });

            var taskDefinition = new FargateTaskDefinition(this, "TaskDefinition", new FargateTaskDefinitionProps {
                Cpu = 256,
                MemoryLimitMiB = 512
            });

            var container = taskDefinition.AddContainer("Container", new ContainerDefinitionOptions
            {
                Image = ContainerImage.FromRegistry("nginx"),
                PortMappings = new PortMapping[]
                {
                    new PortMapping
                    {
                        ContainerPort = 80
                    }
                }
            });

            var ecsService = new FargateService(this, "FargateService", new FargateServiceProps
            {
                Cluster = ecsCluster,
                TaskDefinition = taskDefinition
            });

            var loadBalancer = new ApplicationLoadBalancer(this, "LoadBalancer", new ApplicationLoadBalancerProps
            {
                Vpc = vpc
            });

            ecsService.RegisterLoadBalancerTargets(
                new EcsTarget
                {
                    ContainerName = container.ContainerName,
                    ContainerPort = container.ContainerPort,
                    NewTargetGroupId = "TargetGroup",
                    Listener = ListenerConfig.ApplicationListener(
                        new ApplicationListener(this, "AlbListener", new ApplicationListenerProps
                        {
                            LoadBalancer = loadBalancer,
                            Protocol = ApplicationProtocol.HTTP
                        }))
                });
        }
    }
}
