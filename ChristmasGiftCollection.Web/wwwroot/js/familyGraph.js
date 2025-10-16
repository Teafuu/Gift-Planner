let network = null;
let dotNetRef = null;

export function initializeGraph(containerId, data, dotNetReference) {
    dotNetRef = dotNetReference;

    const container = document.getElementById(containerId);
    if (!container) {
        console.error(`Container with id '${containerId}' not found`);
        return;
    }

    // Configure Vis.js network options with Christmas theme
    const options = {
        nodes: {
            shape: 'dot',
            size: 30,
            font: {
                size: 16,
                face: 'Roboto',
                color: '#ffffff'
            },
            borderWidth: 2,
            shadow: true
        },
        edges: {
            width: 2,
            shadow: true,
            smooth: {
                type: 'continuous'
            },
            font: {
                size: 12,
                align: 'middle'
            }
        },
        groups: {
            adult: {
                color: {
                    background: '#ef5350',  // Christmas red
                    border: '#c62828',
                    highlight: {
                        background: '#ff6659',
                        border: '#c62828'
                    }
                }
            },
            child: {
                color: {
                    background: '#66bb6a',  // Christmas green
                    border: '#388e3c',
                    highlight: {
                        background: '#81c784',
                        border: '#388e3c'
                    }
                }
            }
        },
        physics: {
            enabled: true,
            stabilization: {
                enabled: true,
                iterations: 200
            },
            barnesHut: {
                gravitationalConstant: -8000,
                centralGravity: 0.3,
                springLength: 150,
                springConstant: 0.04
            }
        },
        interaction: {
            hover: true,
            tooltipDelay: 200,
            zoomView: true,
            dragView: true
        }
    };

    // Create the network
    network = new vis.Network(container, data, options);

    // Handle node click events
    network.on('click', function (params) {
        if (params.nodes.length > 0) {
            const nodeId = params.nodes[0];
            if (dotNetRef) {
                dotNetRef.invokeMethodAsync('OnNodeClick', nodeId);
            }
        }
    });

    // Handle node double-click events
    network.on('doubleClick', function (params) {
        if (params.nodes.length > 0) {
            const nodeId = params.nodes[0];
            if (dotNetRef) {
                dotNetRef.invokeMethodAsync('OnNodeDoubleClick', nodeId);
            }
        }
    });

    // Fit the graph to the container after stabilization
    network.once('stabilizationIterationsDone', function () {
        network.fit({
            animation: {
                duration: 1000,
                easingFunction: 'easeInOutQuad'
            }
        });
    });
}

export function updateGraph(data) {
    if (network) {
        network.setData(data);
    }
}

export function fitGraph() {
    if (network) {
        network.fit({
            animation: {
                duration: 500,
                easingFunction: 'easeInOutQuad'
            }
        });
    }
}

export function dispose() {
    if (network) {
        network.destroy();
        network = null;
    }
    dotNetRef = null;
}
