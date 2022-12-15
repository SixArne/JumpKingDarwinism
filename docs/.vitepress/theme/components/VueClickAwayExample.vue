<script>

export default {
    methods: {
        doShit(event) {
            console.log("Hello world")
        }
    }
}
</script>


<template>
<button @click="doShit()">
    Hello world
</button>
</template>
